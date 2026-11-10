using EsServices.ViewModel.Consultation;
using Core = EsConsultationServices.Contracts;

namespace EsServices.Services.Consultation
{
    /// <summary>
    /// Тонкий UI-фасад над EsConsultationServices: мапит DTO нового сервиса
    /// в ViewModel, которые использует Blazor-страница.
    /// </summary>
    public class ConsultationService : IConsultationService
    {
        private readonly Core.IConsultationService consultationService;

        public ConsultationService(Core.IConsultationService consultationService)
        {
            this.consultationService = consultationService;
        }

        public async Task<ConsultationSessionVM> StartAsync(
            int expertSystemId,
            CancellationToken cancellationToken = default)
        {
            var session = await consultationService.StartAsync(expertSystemId, cancellationToken);

            return new ConsultationSessionVM
            {
                SessionId = session.SessionId,
                ExpertSystemId = session.ExpertSystemId,
                ExpertSystemName = session.ExpertSystemName,
                Step = MapStep(session.Step)
            };
        }

        public async Task<ConsultationStep> GetCurrentAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var step = await consultationService.GetCurrentAsync(sessionId, cancellationToken);
            return MapStep(step);
        }

        public async Task<ConsultationStep> AnswerAsync(
            string sessionId,
            string variableName,
            string value,
            CancellationToken cancellationToken = default)
        {
            var step = await consultationService.AnswerAsync(
                sessionId,
                variableName,
                value,
                cancellationToken);

            return MapStep(step);
        }

        public async Task<ConsultationExplanationVM> GetExplanationAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var explanation = await consultationService.GetExplanationAsync(sessionId, cancellationToken);
            return MapExplanation(explanation);
        }

        private static ConsultationStep MapStep(Core.ConsultationStep step)
        {
            return new ConsultationStep
            {
                Question = step.Question == null ? null : MapQuestion(step.Question),
                Result = step.Result == null ? null : MapResult(step.Result)
            };
        }

        private static ConsultationQuestionVM MapQuestion(Core.ConsultationQuestion question)
        {
            return new ConsultationQuestionVM
            {
                Name = question.Name,
                Text = question.Text,
                Answers = question.Answers
                    .Select(answer => new ConsultationAnswerVM
                    {
                        Value = answer.Value,
                        Display = answer.Display
                    })
                    .ToList()
            };
        }

        private static ConsultationResultVM MapResult(Core.ConsultationResult result)
        {
            return new ConsultationResultVM
            {
                VariableName = result.VariableName,
                Value = result.Value,
                Display = result.Display,
                IsUnknown = result.IsUnknown,
                Explanation = MapExplanation(result.Explanation)
            };
        }

        private static ConsultationExplanationVM MapExplanation(Core.ConsultationExplanation explanation)
        {
            return new ConsultationExplanationVM
            {
                Rules = explanation.Rules
                    .Select(rule => new ConsultationExplanationRuleVM
                    {
                        Name = rule.Name,
                        Reasoning = rule.Reasoning,
                        Work = rule.Work,
                        Conditions = rule.Conditions.ToList(),
                        Result = rule.Result
                    })
                    .ToList(),
                ProvedFacts = explanation.ProvedFacts.ToList(),
                UnknownVariables = explanation.UnknownVariables.ToList()
            };
        }
    }
}
