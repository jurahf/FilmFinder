using System.ComponentModel.DataAnnotations;

namespace EsConsultationApi.Contracts;

public sealed class StartConsultationRequest
{
    /// <summary>Id экспертной системы. Необязателен: иначе берётся из конфигурации.</summary>
    public int? ExpertSystemId { get; set; }
}

public sealed class AnswerRequest
{
    /// <summary>Имя переменной из текущего вопроса.</summary>
    [Required]
    public string VariableName { get; set; } = string.Empty;

    /// <summary>Значение из домена переменной.</summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}

public sealed class ReviewRequest
{
    /// <summary>true — понравилось, false — не понравилось.</summary>
    public bool Liked { get; set; }

    public string? Comment { get; set; }
}
