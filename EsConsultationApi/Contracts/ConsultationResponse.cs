using EsConsultationServices.Contracts;

namespace EsConsultationApi.Contracts;

public static class ConsultationStatuses
{
    public const string Question = "Question";
    public const string Completed = "Completed";
    public const string Unknown = "Unknown";
}

public sealed class ConsultationResponse
{
    public string SessionId { get; init; } = string.Empty;

    public int ExpertSystemId { get; init; }
    public string ExpertSystemName { get; init; } = string.Empty;

    /// <summary>Question, Completed или Unknown.</summary>
    public string Status { get; init; } = ConsultationStatuses.Question;

    public ConsultationQuestion? Question { get; init; }
    public ConsultationResult? Result { get; init; }
    public FilmDto? Film { get; init; }
}
