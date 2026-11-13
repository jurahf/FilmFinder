namespace EsConsultationServices.Workflow;

public sealed class ConsultationOptions
{
    public const string SectionName = "Consultation";

    /// <summary>Id экспертной системы, если клиент его не передал. 0 — искать по имени.</summary>
    public int ExpertSystemId { get; set; }

    /// <summary>Имя экспертной системы для поиска, если Id не задан.</summary>
    public string ExpertSystemName { get; set; } = "Фильмы";
}
