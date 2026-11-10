using EsConsultationServices.Contracts;
using EsDb.Model.Enums;

namespace EsConsultationServices.Engine
{
    /// <summary>
    /// Движок консультации. Не зависит от EF, БД и UI.
    /// </summary>
    public class ConsultationEngine
    {
        private const int MaxDepth = 64;
        private const int MaxSteps = 512;

        public ConsultationEngineState Start(ConsultationExpertSystem expertSystem)
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

        /// <summary>
        /// Возвращает текущий вопрос или результат, не продвигая консультацию.
        /// Нужен для продолжения после перезапуска.
        /// </summary>
        public ConsultationStep Current(ConsultationEngineState state)
        {
            if (state.Result != null)
                return new ConsultationStep { Result = state.Result };

            if (state.GoalStack.Count == 0)
            {
                state.Result = CreateResult(state);
                return new ConsultationStep { Result = state.Result };
            }

            return CreateQuestion(state.GoalStack[^1]);
        }

        public ConsultationExplanation BuildExplanation(ConsultationEngineState state)
        {
            var rules = state.WorkedRules
                .OrderBy(x => x.Key.Order)
                .ThenBy(x => x.Key.Name)
                .Select(pair => new ConsultationRuleExplanation
                {
                    Name = pair.Key.Name,
                    Reasoning = pair.Key.Reasoning,
                    Work = pair.Value,
                    Conditions = pair.Key.Conditions
                        .OrderBy(x => x.Order)
                        .Select(x => x.Fact.Display)
                        .ToList(),
                    Result = pair.Key.Result?.Display ?? "(вывод не задан)"
                })
                .ToList();

            var provedFacts = state.Proved
                .Where(x => x.Value == FactTruly.IsTrue)
                .Select(x => x.Key.Display)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var unknownVariables = state.UnknownVariables
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToList();

            return new ConsultationExplanation
            {
                Rules = rules,
                ProvedFacts = provedFacts,
                UnknownVariables = unknownVariables
            };
        }

        private ConsultationStep DoConsult(ConsultationEngineState state, ConsultationVariable goal, int depth)
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
                        Result = new ConsultationResult
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

        private RuleEvaluation EvaluateRule(ConsultationEngineState state, ConsultationRule rule, int depth)
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

        private static ConsultationStep CreateQuestion(ConsultationVariable variable)
        {
            var question = new ConsultationQuestion
            {
                Name = variable.Name,
                Text = string.IsNullOrWhiteSpace(variable.Question)
                    ? $"{variable.Name} = ?"
                    : variable.Question!,
                Answers = variable.Domain?.Values
                    .Select(x => new ConsultationAnswerOption
                    {
                        Value = x.Value,
                        Display = x.Display
                    })
                    .ToList() ?? new List<ConsultationAnswerOption>()
            };

            return new ConsultationStep { Question = question };
        }

        private static ConsultationFact? FindProvedFact(ConsultationEngineState state, ConsultationVariable variable)
        {
            return state.Proved
                .Where(x => x.Value == FactTruly.IsTrue && x.Key.Variable == variable)
                .Select(x => x.Key)
                .FirstOrDefault();
        }

        private static void RecordProved(ConsultationEngineState state, ConsultationFact fact, FactTruly truly)
        {
            state.Proved[fact] = truly;
        }

        /// <summary>
        /// Применяет ответ пользователя к состоянию. Используется и при обычном ответе,
        /// и при восстановлении состояния из ConsultationAnswers.
        /// </summary>
        internal static void RecordAnswer(ConsultationEngineState state, ConsultationVariable variable, string value)
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
                var transient = new ConsultationFact
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

        private static void RemoveGoal(ConsultationEngineState state, ConsultationVariable goal)
        {
            if (state.GoalStack.Count > 0 && ReferenceEquals(state.GoalStack[^1], goal))
                state.GoalStack.RemoveAt(state.GoalStack.Count - 1);
            else
                state.GoalStack.Remove(goal);
        }

        private ConsultationResult CreateResult(ConsultationEngineState state)
        {
            var target = state.ExpertSystem.Target;
            if (target == null)
                return CreateUnknownResult(state, "Цель консультации не задана.");

            var proved = FindProvedFact(state, target);
            if (proved == null)
                return CreateUnknownResult(state, target.Name);

            return new ConsultationResult
            {
                VariableName = target.Name,
                Value = proved.DomainValue.Value,
                Display = proved.DomainValue.Display,
                IsUnknown = false,
                Explanation = BuildExplanation(state)
            };
        }

        private ConsultationResult CreateUnknownResult(ConsultationEngineState state, string variableName)
        {
            return new ConsultationResult
            {
                VariableName = variableName,
                Value = string.Empty,
                Display = "Не удалось вывести значение.",
                IsUnknown = true,
                Explanation = BuildExplanation(state)
            };
        }

        private sealed class RuleEvaluation
        {
            public bool IsTrue { get; private init; }
            public ConsultationQuestion? Question { get; private init; }

            public static RuleEvaluation True { get; } = new() { IsTrue = true };
            public static RuleEvaluation False { get; } = new() { IsTrue = false };

            public static RuleEvaluation WithQuestion(ConsultationQuestion question) =>
                new() { Question = question };
        }
    }
}
