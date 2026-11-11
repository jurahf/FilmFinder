namespace EsConsultationServices.Films;

/// <summary>
/// UI-независимое представление фильма, подобранного по совету или фильтрам.
/// </summary>
public sealed class RecommendedFilm
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public int Year { get; init; }
    public decimal Rating { get; init; }
    public string? Description { get; init; }
    public string? Slogan { get; init; }
    public string? Poster { get; init; }
    public string? Link { get; init; }
    public string? KinopoiskId { get; init; }

    public IReadOnlyList<string> Genres { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Countries { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Actors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Producers { get; init; } = Array.Empty<string>();
    public IReadOnlyList<FilmTag> Tags { get; init; } = Array.Empty<FilmTag>();
}

public sealed class FilmTag
{
    public string Name { get; init; } = string.Empty;
    public int Percent { get; init; }
}

public static class AdviceCatalogConstants
{
    public const string DomainName = "#Советы";
}
