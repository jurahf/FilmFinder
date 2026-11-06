using EsDb.Model.Enums;
using EsServices.ViewModel;
using EsServices.ViewModel.Consultation;

namespace EsServices.Services.Consultation
{
    /// <summary>
    /// In-memory движок консультации. Не зависит от EF и от ClassicClasses.
    /// </summary>
    public class ConsultationEngine
    {
        private const int MaxDepth = 64;
        private const int MaxSteps = 512;

        public ConsultationEngineState Start(ExpertSystemVM expertSystem)
        {
            var state = new ConsultationEngineState(expertSystem);

            if (expertSystem.Target != null)
                state.GoalStack.Add(expertSystem.Target);

            return state;
        }

        public ConsultationStep Next(ConsultationEngineState state)
        {
            if (state.Result != null)
                return new ConsultationStep { Result = state.Result };

            int steps = 0;
            while (state.GoalStack.Count > 0)
            {
                if (steps++ > MaxSteps)
                {
                    state.Result = CreateUnknownResult(state, "Превышено максимальное количество шагов вывода.");
                    return new ConsultationStep { Result = state.Result };
                }

                var goal = state.GoalStack[^1];
                var step = DoConsult(state, goal, 0);

                if (step.Question != null)
                    return step;

                // DoConsult снимает цель со стека. Если остались родительские цели,
                // пересчитываем их заново — уже с новыми доказанными фактами.
                if (state.GoalStack.Count == 0)
                    break;
            }

            state.Result = CreateResult(state);
            return new ConsultationStep { Result = state.Result };
        }

        public ConsultationStep Answer(ConsultationEngineState state, string variableName, string value)
        {
            if (state.Result != null)
                return new ConsultationStep { Result = state.Result };

            if (state.GoalStack.Count == 0)
            {
                state.Result = CreateResult(state);
                return new ConsultationStep { Result = state.Result };
            }

            var goal = state.GoalStack[^1];
            if (!string.Equals(goal.Name, variableName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Ответ относится к переменной '{variableName}', а ожидается ответ для '{goal.Name}'.");
            }

            RecordAnswer(state, goal, value);
            state.GoalStack.RemoveAt(state.GoalStack.Count - 1);

            return Next(state);
        }

        public ConsultationExplanationVM BuildExplanation(ConsultationEngineState state)
        {
            var result = new ConsultationExplanationVM();

            foreach (var pair in state.WorkedRules.OrderBy(x => x.Key.Order).ThenBy(x => x.Key.Name))
            {
                result.Rules.Add(new ConsultationExplanationRuleVM
                {
                    Name = pair.Key.Name,
                    Reasoning = pair.Key.Reasoning,
                    Work = pair.Value,
                    Conditions = pair.Key.Conditions
                        .OrderBy(x => x.Order)
                        .Select(x => x.Fact.Display)
                        .ToList(),
                    Result = pair.Key.Result?.Display ?? "(вывод не задан)"
                });
            }

            result.ProvedFacts = state.Proved
                .Where(x => x.Value == FactTruly.IsTrue)
                .Select(x => x.Key.Display)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            result.UnknownVariables = state.UnknownVariables
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToList();

            return result;
        }

        private ConsultationStep DoConsult(ConsultationEngineState state, VariableVM goal, int depth)
        {
            if (depth > MaxDepth)
            {
                RemoveGoal(state, goal);
                state.UnknownVariables.Add(goal);
                return new ConsultationStep { Result = CreateUnknownResult(state, goal.Name) };
            }

            if (goal.Type == VariableType.Queriable)
                return CreateQuestion(goal);

            var rules = state.ExpertSystem.Rules
                .Where(x => x.Result?.Variable == goal)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name)
                .ToList();

            foreach (var rule in rules)
            {
                var evaluation = EvaluateRule(state, rule, depth + 1);

                if (evaluation.Question != null)
                    return new ConsultationStep { Question = evaluation.Question };

                if (evaluation.IsTrue)
                {
                    state.WorkedRules[rule] = RuleWork.Signify;

                    if (rule.Result != null)
                        RecordProved(state, rule.Result, FactTruly.IsTrue);

                    RemoveGoal(state, goal);

                    return new ConsultationStep
                    {
                        Result = new ConsultationResultVM
                        {
                            VariableName = goal.Name,
                            Value = rule.Result?.DomainValue.Value ?? string.Empty,
                            Display = rule.Result?.DomainValue.Display ?? string.Empty
                        }
                    };
                }

                state.WorkedRules[rule] = RuleWork.UnSignify;
            }

            // Для выводимо-запрашиваемой переменной, если ни одно правило не сработало,
            // запрашиваем значение у пользователя.
            if (goal.Type == VariableType.QuerriableDetective)
                return CreateQuestion(goal);

            RemoveGoal(state, goal);
            state.UnknownVariables.Add(goal);
            return new ConsultationStep { Result = CreateUnknownResult(state, goal.Name) };
        }

        private RuleEvaluation EvaluateRule(ConsultationEngineState state, RuleVM rule, int depth)
        {
            foreach (var condition in rule.Conditions.OrderBy(x => x.Order))
            {
                var fact = condition.Fact;
                var proved = FindProvedFact(state, fact.Variable);

                if (proved != null)
                {
                    if (string.Equals(proved.DomainValue.Value, fact.DomainValue.Value, StringComparison.Ordinal))
                        continue;

                    return RuleEvaluation.False;
                }

                state.GoalStack.Add(fact.Variable);
                var step = DoConsult(state, fact.Variable, depth + 1);

                if (step.Question != null)
                    return RuleEvaluation.WithQuestion(step.Question);

                var after = FindProvedFact(state, fact.Variable);
                if (after != null &&
                    string.Equals(after.DomainValue.Value, fact.DomainValue.Value, StringComparison.Ordinal))
                {
                    continue;
                }

                return RuleEvaluation.False;
            }

            return RuleEvaluation.True;
        }

        private static ConsultationStep CreateQuestion(VariableVM variable)
        {
            var question = new ConsultationQuestionVM
            {
                Name = variable.Name,
                Text = string.IsNullOrWhiteSpace(variable.Question)
                    ? $"{variable.Name} = ?"
                    : variable.Question!
            };

            if (variable.Domain != null)
            {
                question.Answers = variable.Domain.Values
                    .Select(x => new ConsultationAnswerVM
                    {
                        Value = x.Value,
                        Display = x.Display
                    })
                    .ToList();
            }

            return new ConsultationStep { Question = question };
        }

        private static FactVM? FindProvedFact(ConsultationEngineState state, VariableVM variable)
        {
            return state.Proved
                .Where(x => x.Value == FactTruly.IsTrue && x.Key.Variable == variable)
                .Select(x => x.Key)
                .FirstOrDefault();
        }

        private static void RecordProved(ConsultationEngineState state, FactVM fact, FactTruly truly)
        {
            state.Proved[fact] = truly;
        }

        private static void RecordAnswer(ConsultationEngineState state, VariableVM variable, string value)
        {
            foreach (var fact in state.ExpertSystem.Facts.Where(x => x.Variable == variable))
            {
                state.Proved[fact] = string.Equals(fact.DomainValue.Value, value, StringComparison.Ordinal)
                    ? FactTruly.IsTrue
                    : FactTruly.IsFalse;
            }

            var domainValue = variable.Domain?.Values
                .FirstOrDefault(x => string.Equals(x.Value, value, StringComparison.Ordinal));

            if (domainValue == null)
                return;

            var existed = state.ExpertSystem.Facts.Any(x =>
                x.Variable == variable && ReferenceEquals(x.DomainValue, domainValue));

            if (!existed)
            {
                var transient = new FactVM
                {
                    Variable = variable,
                    DomainValue = domainValue
                };

                state.TransientFacts.Add(transient);
                state.Proved[transient] = FactTruly.IsTrue;
            }
            else
            {
                var fact = state.ExpertSystem.Facts.First(x =>
                    x.Variable == variable && ReferenceEquals(x.DomainValue, domainValue));
                state.Proved[fact] = FactTruly.IsTrue;
            }
        }

        private static void RemoveGoal(ConsultationEngineState state, VariableVM goal)
        {
            if (state.GoalStack.Count > 0 && ReferenceEquals(state.GoalStack[^1], goal))
                state.GoalStack.RemoveAt(state.GoalStack.Count - 1);
            else
                state.GoalStack.Remove(goal);
        }

        private ConsultationResultVM CreateResult(ConsultationEngineState state)
        {
            var target = state.ExpertSystem.Target;
            if (target == null)
                return CreateUnknownResult(state, "Цель консультации не задана.");

            var proved = FindProvedFact(state, target);
            if (proved == null)
                return CreateUnknownResult(state, target.Name);

            return new ConsultationResultVM
            {
                VariableName = target.Name,
                Value = proved.DomainValue.Value,
                Display = proved.DomainValue.Display,
                IsUnknown = false,
                Explanation = BuildExplanation(state)
            };
        }

        private ConsultationResultVM CreateUnknownResult(ConsultationEngineState state, string variableName)
        {
            return new ConsultationResultVM
            {
                VariableName = variableName,
                Value = string.Empty,
                Display = "Не удалось вывести значение.",
                IsUnknown = true,
                Explanation = BuildExplanation(state)
            };
        }

        private class RuleEvaluation
        {
            public bool IsTrue { get; private init; }
            public ConsultationQuestionVM? Question { get; private init; }

            public static RuleEvaluation True { get; } = new() { IsTrue = true };
            public static RuleEvaluation False { get; } = new() { IsTrue = false };

            public static RuleEvaluation WithQuestion(ConsultationQuestionVM question) =>
                new() { Question = question };
        }
    }
}
