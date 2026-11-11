using EsServices.ViewModel.Film;
using FilmDb;
using Microsoft.EntityFrameworkCore;

namespace EsServices.Services.CustomProperties
{
    public class CustomPropertiesService : ICustomPropertiesService
    {
        private readonly FilmDbContext db;

        public CustomPropertiesService(FilmDbContext db)
        {
            this.db = db;
        }

        public async Task<List<FilmLookupItemVM>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await db.CustomProperties
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new FilmLookupItemVM { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
        }
    }
}
