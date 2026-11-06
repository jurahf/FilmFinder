using EsServices.ViewModel.Consultation;

namespace EsServices.Services.Consultation
{
    public interface IConsultationService
    {
        Task<ConsultationSessionVM> StartAsync(int expertSystemId, CancellationToken cancellationToken = default);
        Task<ConsultationStep> AnswerAsync(
            string sessionId,
            string variableName,
            string value,
            CancellationToken cancellationToken = default);
        Task<ConsultationExplanationVM> GetExplanationAsync(string sessionId, CancellationToken cancellationToken = default);
    }

    public class ConsultationSessionVM
    {
        public string SessionId { get; init; } = string.Empty;
        public int ExpertSystemId { get; init; }
        public string ExpertSystemName { get; init; } = string.Empty;
        public ConsultationStep Step { get; init; } = new();
        public string? PersistenceError { get; init; }
    }
}
