using EsConsultationServices.Contracts;
using EsConsultationServices.Engine;
using EsConsultationServices.Persistence;
using EsDb;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationServices.Services
{
    /// <summary>
    /// Прикладной сервис консультации: транзакции, проверки и orchestration движка.
    /// Состояние каждой сессии читается из EsDb, граф ЭС — из общего кэша.
    /// </summary>
    public sealed class ConsultationService : IConsultationService
    {
        private readonly EsDbContext db;
        private readonly ConsultationEngine engine;
        private readonly ConsultationStateStore store;
        private readonly ConsultationExpertSystemCache expertSystemCache;

        internal ConsultationService(
            EsDbContext db,
            ConsultationEngine engine,
            ConsultationStateStore store,
            ConsultationExpertSystemCache expertSystemCache)
        {
            this.db = db;
            this.engine = engine;
            this.store = store;
            this.expertSystemCache = expertSystemCache;
        }

        public async Task<ConsultationSession> StartAsync(
            int expertSystemId,
            CancellationToken cancellationToken = default)
        {
            // Прогреваем кэш до открытия транзакции, чтобы загрузка графа
            // не выполнялась внутри неё.
            await expertSystemCache.GetAsync(expertSystemId, cancellationToken);

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var state = await store.CreateAsync(expertSystemId, cancellationToken);
            var step = engine.Next(state.EngineState);

            await store.SaveAsync(state, null, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ConsultationSession
            {
                SessionId = state.SessionId,
                ExpertSystemId = state.ExpertSystemId,
                ExpertSystemName = state.ExpertSystemName,
                Step = step
            };
        }

        public async Task<ConsultationStep> GetCurrentAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var state = await LoadRequiredAsync(sessionId, cancellationToken);
            return engine.Current(state.EngineState);
        }

        public async Task<ConsultationStep> AnswerAsync(
            string sessionId,
            string variableName,
            string value,
            CancellationToken cancellationToken = default)
        {
            var state = await LoadRequiredAsync(sessionId, cancellationToken);

            if (state.EngineState.Result != null || state.EngineState.GoalStack.Count == 0)
            {
                throw new InvalidOperationException(
                    "Консультация уже завершена. Начните новую консультацию.");
            }

            var variable = state.EngineState.ExpertSystem.Variables
                .FirstOrDefault(x => string.Equals(x.Name, variableName, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(
                    $"Переменная '{variableName}' не принадлежит экспертной системе консультации.");

            var expected = state.EngineState.GoalStack[^1];
            if (!string.Equals(expected.Name, variable.Name, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Ответ относится к переменной '{variableName}', а ожидается ответ для '{expected.Name}'.");
            }

            var domainValue = variable.Domain?.Values
                .FirstOrDefault(x => string.Equals(x.Value, value, StringComparison.Ordinal));

            if (variable.Domain != null && domainValue == null)
            {
                throw new InvalidOperationException(
                    $"Значение '{value}' отсутствует в домене переменной '{variable.Name}'.");
            }

            var pendingAnswer = new PendingAnswer(variable.Id, domainValue?.Id, value);

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            ConsultationStep step;
            try
            {
                step = engine.Answer(state.EngineState, variableName, value);
                await store.SaveAsync(state, pendingAnswer, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "Не удалось сохранить ответ: состояние консультации изменилось. " +
                    "Запросите текущий шаг и повторите ответ.", ex);
            }

            return step;
        }

        public async Task<ConsultationExplanation> GetExplanationAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var state = await LoadRequiredAsync(sessionId, cancellationToken);
            return engine.BuildExplanation(state.EngineState);
        }

        private async Task<ConsultationSessionState> LoadRequiredAsync(
            string sessionId,
            CancellationToken cancellationToken)
        {
            return await store.LoadAsync(sessionId, cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"Консультация с идентификатором сессии '{sessionId}' не найдена.");
        }
    }
}
