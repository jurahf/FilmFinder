using EsServices.ViewModel.Film;
using FilmDb;
using Microsoft.EntityFrameworkCore;

namespace EsServices.Services.Genres
{
    public class GenresService : IGenresService
    {
        private readonly FilmDbContext db;

        public GenresService(FilmDbContext db)
        {
            this.db = db;
        }

        public async Task<List<FilmLookupItemVM>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await db.Genres
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new FilmLookupItemVM { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
        }
    }
}
