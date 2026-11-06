using EsDb;
using EsDb.Model;
using EsServices.Services;
using EsServices.ViewModel.Consultation;
using Microsoft.EntityFrameworkCore;
using ConsultationEntity = EsDb.Model.Consultation;

namespace EsServices.Services.Consultation
{
    public class ConsultationService : IConsultationService
    {
        private readonly EsDbContext db;
        private readonly IExpertSystemService expertSystemService;
        private readonly ConsultationEngine engine;
        private readonly ConsultationSessionStore store;

        public ConsultationService(
            EsDbContext db,
            IExpertSystemService expertSystemService,
            ConsultationEngine engine,
            ConsultationSessionStore store)
        {
            this.db = db;
            this.expertSystemService = expertSystemService;
            this.engine = engine;
            this.store = store;
        }

        public async Task<ConsultationSessionVM> StartAsync(int expertSystemId, CancellationToken cancellationToken = default)
        {
            var expertSystem = await expertSystemService.GetAsync(expertSystemId, cancellationToken);
            var sessionId = Guid.NewGuid().ToString("N");

            var state = new ConsultationSessionState(
                sessionId,
                expertSystem,
                engine.Start(expertSystem));

            var step = engine.Next(state.EngineState);
            await PersistIfCompletedAsync(state, cancellationToken);
            step = WithWarning(step, state.PersistenceError);

            store.Add(state);

            return new ConsultationSessionVM
            {
                SessionId = sessionId,
                ExpertSystemId = expertSystem.Id,
                ExpertSystemName = expertSystem.Name,
                Step = step,
                PersistenceError = state.PersistenceError
            };
        }

        public async Task<ConsultationStep> AnswerAsync(
            string sessionId,
            string variableName,
            string value,
            CancellationToken cancellationToken = default)
        {
            var state = store.Get(sessionId);
            var step = engine.Answer(state.EngineState, variableName, value);
            await PersistIfCompletedAsync(state, cancellationToken);
            return WithWarning(step, state.PersistenceError);
        }

        public Task<ConsultationExplanationVM> GetExplanationAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var state = store.Get(sessionId);
            return Task.FromResult(engine.BuildExplanation(state.EngineState));
        }

        private async Task PersistIfCompletedAsync(
            ConsultationSessionState state,
            CancellationToken cancellationToken)
        {
            if (state.ConsultationId.HasValue || state.EngineState.Result == null)
                return;

            try
            {
                var expertSystem = await db.ExpertSystems
                    .FirstOrDefaultAsync(x => x.Id == state.ExpertSystem.Id, cancellationToken);

                if (expertSystem == null)
                    throw new InvalidOperationException("Экспертная система для консультации не найдена.");

                var session = new Session
                {
                    SessionId = state.SessionId,
                    CreateDate = DateTime.Now,
                    LastActivityDate = DateTime.Now
                };

                var consultation = new ConsultationEntity
                {
                    Session = session,
                    ExpertSystem = expertSystem
                };

                db.Consultations.Add(consultation);
                await db.SaveChangesAsync(cancellationToken);

                var result = state.EngineState.Result;
                if (!result.IsUnknown && !string.IsNullOrWhiteSpace(result.Value))
                {
                    db.FinalSolutions.Add(new FinalSolution
                    {
                        ConsultationId = consultation.Id,
                        VariableName = result.VariableName,
                        Value = result.Value
                    });
                }

                foreach (var pair in state.EngineState.Proved)
                {
                    if (pair.Key.Id <= 0 || state.EngineState.TransientFacts.Contains(pair.Key))
                        continue;

                    db.ConsultationFacts.Add(new ConsultationFact
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

                    db.ConsultationRules.Add(new ConsultationRule
                    {
                        ConsultationId = consultation.Id,
                        RuleId = pair.Key.Id,
                        Work = pair.Value
                    });
                }

                await db.SaveChangesAsync(cancellationToken);
                state.ConsultationId = consultation.Id;
            }
            catch (Exception ex)
            {
                state.PersistenceError = $"Не удалось сохранить консультацию: {ex.Message}";
            }
        }

        private static ConsultationStep WithWarning(ConsultationStep step, string? warning)
        {
            if (string.IsNullOrWhiteSpace(warning))
                return step;

            return new ConsultationStep
            {
                Question = step.Question,
                Result = step.Result,
                Warning = warning
            };
        }
    }
}
