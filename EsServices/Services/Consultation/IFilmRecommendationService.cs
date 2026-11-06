using EsServices.ViewModel.Film;

namespace EsServices.Services.Consultation
{
    public interface IFilmRecommendationService
    {
        Task<List<FilmLookupItemVM>> GetGenresAsync(CancellationToken cancellationToken = default);
        Task<List<FilmLookupItemVM>> GetCustomPropertiesAsync(CancellationToken cancellationToken = default);

        Task<FilmResultVM?> FindByAdviceAsync(Guid adviceKey, CancellationToken cancellationToken = default);
        Task<FilmResultVM?> FindByFiltersAsync(
            IReadOnlyCollection<int> genreIds,
            IReadOnlyCollection<int> customPropertyIds,
            CancellationToken cancellationToken = default);
    }
}
