using EsDb.Model.Enums;

namespace EsConsultationServices.Engine;

public sealed class ConsultationEngineState
{
    public ConsultationEngineState(ConsultationExpertSystem expertSystem)
    {
        ExpertSystem = expertSystem;
    }

    public ConsultationExpertSystem ExpertSystem { get; }

    /// <summary>Стек целей: последний элемент — текущая цель.</summary>
    public List<ConsultationVariable> GoalStack { get; } = new();

    /// <summary>Доказанные факты и их истинность.</summary>
    public Dictionary<ConsultationFact, FactTruly> Proved { get; } = new();

    /// <summary>Факты, созданные из ответов пользователя и не имеющие записи в БД.</summary>
    public HashSet<ConsultationFact> TransientFacts { get; } = new();

    /// <summary>Сработавшие и проверенные правила.</summary>
    public Dictionary<ConsultationRule, RuleWork> WorkedRules { get; } = new();

    public HashSet<ConsultationVariable> UnknownVariables { get; } = new();

    public Contracts.ConsultationResult? Result { get; set; }
}
