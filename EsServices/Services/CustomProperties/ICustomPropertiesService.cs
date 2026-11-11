using EsServices.ViewModel.Film;

namespace EsServices.Services.CustomProperties
{
    /// <summary>Справочник свойств фильмов для UI.</summary>
    public interface ICustomPropertiesService
    {
        Task<List<FilmLookupItemVM>> GetListAsync(CancellationToken cancellationToken = default);
    }
}
