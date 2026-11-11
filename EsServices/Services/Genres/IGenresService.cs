using EsServices.ViewModel.Film;

namespace EsServices.Services.Genres
{
    /// <summary>Справочник жанров для UI.</summary>
    public interface IGenresService
    {
        Task<List<FilmLookupItemVM>> GetListAsync(CancellationToken cancellationToken = default);
    }
}
