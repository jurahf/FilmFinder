using EsConsultationServices.Contracts;
using EsConsultationServices.Films;

namespace EsConsultationServices.Workflow;

public enum ConsultationFlowStatus
{
    Question,
    Completed,
    Unknown
}

/// <summary>
/// Результат шага консультации, не зависящий от транспорта (HTTP или VK).
/// </summary>
public sealed class ConsultationFlowResult
{
    public string SessionId { get; init; } = string.Empty;
    public int ExpertSystemId { get; init; }
    public string ExpertSystemName { get; init; } = string.Empty;
    public ConsultationFlowStatus Status { get; init; }
    public ConsultationStep Step { get; init; } = new();
    public RecommendedFilm? Film { get; init; }
}

public interface IConsultationWorkflow
{
    Task<ConsultationFlowResult> StartAsync(
        int? expertSystemId,
        CancellationToken cancellationToken = default);

    Task<ConsultationFlowResult> GetCurrentAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<ConsultationFlowResult> AnswerAsync(
        string sessionId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default);

    Task<ConsultationFlowResult> GetAnotherAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task ReviewAsync(
        string sessionId,
        bool liked,
        string? comment,
        CancellationToken cancellationToken = default);
}
