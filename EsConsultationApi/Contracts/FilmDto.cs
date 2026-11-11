namespace EsConsultationApi.Contracts;

public sealed class FilmDto
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
    public IReadOnlyList<FilmTagDto> Tags { get; init; } = Array.Empty<FilmTagDto>();
}

public sealed class FilmTagDto
{
    public string Name { get; init; } = string.Empty;
    public int Percent { get; init; }
}
