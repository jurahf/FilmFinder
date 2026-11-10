namespace EsConsultationServices.Contracts;

public sealed class ConsultationStep
{
    public ConsultationQuestion? Question { get; init; }
    public ConsultationResult? Result { get; init; }
    public bool IsCompleted => Result != null;
}

public sealed class ConsultationQuestion
{
    public string Name { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public IReadOnlyList<ConsultationAnswerOption> Answers { get; init; } = Array.Empty<ConsultationAnswerOption>();
}

public sealed class ConsultationAnswerOption
{
    public string Value { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
}

public sealed class ConsultationResult
{
    public string VariableName { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public bool IsUnknown { get; init; }
    public ConsultationExplanation Explanation { get; init; } = new();
}

public sealed class ConsultationExplanation
{
    public IReadOnlyList<ConsultationRuleExplanation> Rules { get; init; } = Array.Empty<ConsultationRuleExplanation>();
    public IReadOnlyList<string> ProvedFacts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UnknownVariables { get; init; } = Array.Empty<string>();
}

public sealed class ConsultationRuleExplanation
{
    public string Name { get; init; } = string.Empty;
    public string? Reasoning { get; init; }
    public EsDb.Model.Enums.RuleWork Work { get; init; }
    public IReadOnlyList<string> Conditions { get; init; } = Array.Empty<string>();
    public string Result { get; init; } = string.Empty;
}

public sealed class ConsultationSession
{
    public string SessionId { get; init; } = string.Empty;
    public int ExpertSystemId { get; init; }
    public string ExpertSystemName { get; init; } = string.Empty;
    public ConsultationStep Step { get; init; } = new();
}

public sealed class ConsultationDisplayValue
{
    public string DomainName { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
}

public interface IConsultationDisplayProvider
{
    Task<IReadOnlyList<ConsultationDisplayValue>> GetDisplayValuesAsync(CancellationToken cancellationToken = default);
}

public interface IConsultationService
{
    Task<ConsultationSession> StartAsync(int expertSystemId, CancellationToken cancellationToken = default);

    Task<ConsultationStep> GetCurrentAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<ConsultationStep> AnswerAsync(
        string sessionId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default);

    Task<ConsultationExplanation> GetExplanationAsync(string sessionId, CancellationToken cancellationToken = default);
}
