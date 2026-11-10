using EsDb.Model.Enums;

namespace EsConsultationServices.Engine
{
    /// <summary>
    /// Неизменяемая доменная модель ЭС для движка консультации.
    /// Один экземпляр переиспользуется всеми сессиями и потоками, поэтому менять его нельзя.
    /// </summary>
    public sealed class ConsultationExpertSystem
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public ConsultationVariable? Target { get; init; }
        public IReadOnlyList<ConsultationVariable> Variables { get; init; } = Array.Empty<ConsultationVariable>();
        public IReadOnlyList<ConsultationRule> Rules { get; init; } = Array.Empty<ConsultationRule>();
        public IReadOnlyList<ConsultationFact> Facts { get; init; } = Array.Empty<ConsultationFact>();
    }

    public sealed class ConsultationVariable
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Question { get; init; }
        public string? Reasoning { get; init; }
        public VariableType Type { get; init; }
        public ConsultationDomain? Domain { get; init; }

        public override string ToString() => Name;
    }

    public sealed class ConsultationDomain
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public IReadOnlyList<ConsultationDomainValue> Values { get; init; } = Array.Empty<ConsultationDomainValue>();
    }

    public sealed class ConsultationDomainValue
    {
        public int Id { get; init; }
        public string Value { get; init; } = string.Empty;
        public string Display { get; init; } = string.Empty;
    }

    public sealed class ConsultationFact
    {
        public int Id { get; init; }
        public ConsultationVariable Variable { get; init; } = null!;
        public ConsultationDomainValue DomainValue { get; init; } = null!;

        public string Display => $"{Variable.Name} = {DomainValue.Display}";

        public override string ToString() => Display;
    }

    public sealed class ConsultationRule
    {
        public int Id { get; init; }
        public int Order { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Reasoning { get; init; }
        public ConsultationFact? Result { get; init; }
        public IReadOnlyList<ConsultationRuleFact> Conditions { get; init; } = Array.Empty<ConsultationRuleFact>();
    }

    public sealed class ConsultationRuleFact
    {
        public int Order { get; init; }
        public ConsultationFact Fact { get; init; } = null!;
    }
}
