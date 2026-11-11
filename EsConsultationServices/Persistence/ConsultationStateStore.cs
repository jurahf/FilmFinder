using EsConsultationServices.Engine;
using EsDb;
using EsDb.Model;
using Microsoft.EntityFrameworkCore;

using DbConsultationFact = EsDb.Model.ConsultationFact;
using DbConsultationRule = EsDb.Model.ConsultationRule;

namespace EsConsultationServices.Persistence
{
    /// <summary>
    /// Загрузка и сохранение состояния консультации в EsDb.
    /// Граф ЭС берётся из ConsultationExpertSystemCache, из БД читается только сессия.
    /// </summary>
    internal sealed class ConsultationStateStore
    {
        private readonly EsDbContext db;
        private readonly ConsultationExpertSystemCache expertSystemCache;

        public ConsultationStateStore(EsDbContext db, ConsultationExpertSystemCache expertSystemCache)
        {
            this.db = db;
            this.expertSystemCache = expertSystemCache;
        }

        public async Task<ConsultationSessionState> CreateAsync(
            int expertSystemId,
            CancellationToken cancellationToken)
        {
            var expertSystem = await expertSystemCache.GetAsync(expertSystemId, cancellationToken);

            if (expertSystem.Target == null)
            {
                throw new InvalidOperationException(
                    "В экспертной системе не выбрана цель консультации.");
            }

            var session = new Session
            {
                SessionId = Guid.NewGuid().ToString("N"),
                CreateDate = DateTime.Now,
                LastActivityDate = DateTime.Now
            };

            var consultation = new Consultation
            {
                Session = session,
                ExpertSystemId = expertSystemId
            };

            db.Sessions.Add(session);
            db.Consultations.Add(consultation);
            await db.SaveChangesAsync(cancellationToken);

            var state = new ConsultationEngineState(expertSystem);
            if (expertSystem.Target != null)
                state.GoalStack.Add(expertSystem.Target);

            return new ConsultationSessionState(
                session.SessionId,
                consultation.Id,
                expertSystem.Id,
                expertSystem.Name,
                state);
        }

        public async Task<ConsultationSessionState?> LoadAsync(
            string sessionId,
            CancellationToken cancellationToken)
        {
            var consultation = await db.Consultations
                .AsNoTracking()
                .Include(x => x.Session)
                .Include(x => x.FinalSolution)
                .Include(x => x.ProvedFacts)
                .Include(x => x.WorkedRules)
                .Include(x => x.Answers)
                .Include(x => x.UnknownVariables)
                .FirstOrDefaultAsync(x => x.Session.SessionId == sessionId, cancellationToken);

            if (consultation == null)
                return null;

            var expertSystem = await expertSystemCache.GetAsync(consultation.ExpertSystemId, cancellationToken);
            var state = new ConsultationEngineState(expertSystem);

            await RestoreGoalStackAsync(state, consultation.GoalStackId, cancellationToken);

            foreach (var provedFact in consultation.ProvedFacts)
            {
                var fact = expertSystem.Facts.FirstOrDefault(x => x.Id == provedFact.FactId);
                if (fact != null)
                    state.Proved[fact] = provedFact.Truly;
            }

            foreach (var workedRule in consultation.WorkedRules.OrderBy(x => x.Id))
            {
                var rule = expertSystem.Rules.FirstOrDefault(x => x.Id == workedRule.RuleId);
                if (rule != null)
                    state.WorkedRules[rule] = workedRule.Work;
            }

            // Ответы проигрываем после фактов: так восстанавливаются и факты,
            // для которых не было записи Fact (транзитные).
            foreach (var answer in consultation.Answers.OrderBy(x => x.Order))
            {
                var variable = expertSystem.Variables.FirstOrDefault(x => x.Id == answer.VariableId);
                if (variable != null)
                    ConsultationEngine.RecordAnswer(state, variable, answer.Value);
            }

            foreach (var unknownVariable in consultation.UnknownVariables)
            {
                var variable = expertSystem.Variables.FirstOrDefault(x => x.Id == unknownVariable.VariableId);
                if (variable != null)
                    state.UnknownVariables.Add(variable);
            }

            return new ConsultationSessionState(
                consultation.Session.SessionId,
                consultation.Id,
                expertSystem.Id,
                expertSystem.Name,
                state);
        }

        public async Task SaveAsync(
            ConsultationSessionState state,
            PendingAnswer? answer,
            CancellationToken cancellationToken)
        {
            var consultation = await db.Consultations
                .Include(x => x.Session)
                .FirstOrDefaultAsync(x => x.Id == state.ConsultationId, cancellationToken)
                ?? throw new InvalidOperationException("Консультация не найдена в базе данных.");

            consultation.Session.LastActivityDate = DateTime.Now;

            await RemoveExistingStateAsync(consultation, cancellationToken);
            await SaveGoalStackAsync(consultation, state.EngineState.GoalStack, cancellationToken);

            if (answer != null)
            {
                var nextOrder = await db.ConsultationAnswers
                    .Where(x => x.ConsultationId == consultation.Id)
                    .MaxAsync(x => (int?)x.Order, cancellationToken) ?? -1;

                db.ConsultationAnswers.Add(new ConsultationAnswer
                {
                    ConsultationId = consultation.Id,
                    VariableId = answer.VariableId,
                    DomainValueId = answer.DomainValueId,
                    Value = answer.Value,
                    Order = nextOrder + 1
                });
            }

            foreach (var pair in state.EngineState.Proved)
            {
                if (pair.Key.Id <= 0 || state.EngineState.TransientFacts.Contains(pair.Key))
                    continue;

                db.ConsultationFacts.Add(new DbConsultationFact
                {
                    ConsultationId = consultation.Id,
                    FactId = pair.Key.Id,
                    Truly = pair.Value
                });
            }

            foreach (var pair in state.EngineState.WorkedRules)
            {
                if (pair.Key.Id <= 0)
                    continue;

                db.ConsultationRules.Add(new DbConsultationRule
                {
                    ConsultationId = consultation.Id,
                    RuleId = pair.Key.Id,
                    Work = pair.Value
                });
            }

            foreach (var unknownVariable in state.EngineState.UnknownVariables)
            {
                if (unknownVariable.Id <= 0)
                    continue;

                db.ConsultationUnknownVariables.Add(new ConsultationUnknownVariable
                {
                    ConsultationId = consultation.Id,
                    VariableId = unknownVariable.Id
                });
            }

            await SaveFinalSolutionAsync(consultation.Id, state.EngineState.Result, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task RemoveExistingStateAsync(
            Consultation consultation,
            CancellationToken cancellationToken)
        {
            var oldGoalIds = await ReadGoalChainIdsAsync(consultation.GoalStackId, cancellationToken);

            consultation.GoalStackId = null;
            consultation.GoalStack = null;

            var facts = await db.ConsultationFacts
                .Where(x => x.ConsultationId == consultation.Id)
                .ToListAsync(cancellationToken);
            var rules = await db.ConsultationRules
                .Where(x => x.ConsultationId == consultation.Id)
                .ToListAsync(cancellationToken);
            var unknownVariables = await db.ConsultationUnknownVariables
                .Where(x => x.ConsultationId == consultation.Id)
                .ToListAsync(cancellationToken);

            db.ConsultationFacts.RemoveRange(facts);
            db.ConsultationRules.RemoveRange(rules);
            db.ConsultationUnknownVariables.RemoveRange(unknownVariables);

            // Сначала сохраняем снятую ссылку на GoalStack и удаление зависимых строк,
            // только потом удаляем сами цели: SQL Server не даст удалить ссылаемую строку.
            await db.SaveChangesAsync(cancellationToken);

            if (oldGoalIds.Count == 0)
                return;

            var oldGoals = await db.GoalStacks
                .Where(x => oldGoalIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            // Ссылки NextGoalId образуют цепочку с NoAction, поэтому сначала
            // разрываем её, а уже потом удаляем строки.
            foreach (var goal in oldGoals)
            {
                goal.NextGoalId = null;
                goal.NextGoal = null;
            }

            await db.SaveChangesAsync(cancellationToken);

            db.GoalStacks.RemoveRange(oldGoals);
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task SaveGoalStackAsync(
            Consultation consultation,
            IReadOnlyList<ConsultationVariable> goalStack,
            CancellationToken cancellationToken)
        {
            GoalStack? next = null;

            foreach (var variable in goalStack.AsEnumerable().Reverse())
            {
                if (variable.Id <= 0)
                    throw new InvalidOperationException(
                        $"Нельзя сохранить цель '{variable.Name}' без записи в базе данных.");

                var goal = new GoalStack
                {
                    VariableId = variable.Id,
                    NextGoal = next
                };

                db.GoalStacks.Add(goal);
                next = goal;
            }

            await db.SaveChangesAsync(cancellationToken);

            consultation.GoalStackId = next?.Id;
            consultation.GoalStack = next;
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task SaveFinalSolutionAsync(
            int consultationId,
            Contracts.ConsultationResult? result,
            CancellationToken cancellationToken)
        {
            var finalSolution = await db.FinalSolutions
                .FirstOrDefaultAsync(x => x.ConsultationId == consultationId, cancellationToken);

            if (result == null || result.IsUnknown || string.IsNullOrWhiteSpace(result.Value))
            {
                if (finalSolution != null)
                    db.FinalSolutions.Remove(finalSolution);

                return;
            }

            if (finalSolution == null)
            {
                db.FinalSolutions.Add(new FinalSolution
                {
                    ConsultationId = consultationId,
                    VariableName = result.VariableName,
                    Value = result.Value
                });

                return;
            }

            finalSolution.VariableName = result.VariableName;
            finalSolution.Value = result.Value;
        }

        private async Task RestoreGoalStackAsync(
            ConsultationEngineState state,
            int? goalStackId,
            CancellationToken cancellationToken)
        {
            var currentId = goalStackId;

            while (currentId.HasValue)
            {
                var goal = await db.GoalStacks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == currentId.Value, cancellationToken);

                if (goal == null)
                    break;

                if (goal.VariableId.HasValue)
                {
                    var variable = state.ExpertSystem.Variables
                        .FirstOrDefault(x => x.Id == goal.VariableId.Value);

                    if (variable != null)
                        state.GoalStack.Add(variable);
                }

                currentId = goal.NextGoalId;
            }
        }

        private async Task<List<int>> ReadGoalChainIdsAsync(
            int? goalStackId,
            CancellationToken cancellationToken)
        {
            var result = new List<int>();
            var currentId = goalStackId;

            while (currentId.HasValue)
            {
                result.Add(currentId.Value);

                var nextId = await db.GoalStacks
                    .AsNoTracking()
                    .Where(x => x.Id == currentId.Value)
                    .Select(x => x.NextGoalId)
                    .FirstOrDefaultAsync(cancellationToken);

                currentId = nextId;
            }

            return result;
        }
    }
}
