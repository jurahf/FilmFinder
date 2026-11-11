namespace EsConsultationServices.Films;

public interface IFilmRecommendationService
{
    /// <summary>
    /// Подбирает фильм по совету: сначала явные фильмы совета, затем похожие по свойствам.
    /// <paramref name="excludedFilmIds"/> — уже показанные фильмы, они не повторяются.
    /// </summary>
    Task<RecommendedFilm?> FindByAdviceAsync(
        Guid adviceKey,
        IReadOnlyCollection<int>? excludedFilmIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Подбирает фильм по жанрам и свойствам, исключая уже показанные.
    /// </summary>
    Task<RecommendedFilm?> FindByFiltersAsync(
        IReadOnlyCollection<int> genreIds,
        IReadOnlyCollection<int> customPropertyIds,
        IReadOnlyCollection<int>? excludedFilmIds = null,
        CancellationToken cancellationToken = default);
}
