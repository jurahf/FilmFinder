using CommonRepositories;
using FilmDb.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Repositories
{
    public interface IFilmRepository : IRepository<Film>
    {
        Task<List<Film>> SearchByName(string query, int limit, int page);
    }


    public class FilmRepository : BaseRepository<Film>, IFilmRepository
    {
        public FilmRepository(FilmDbContext dbContext) 
            : base(dbContext)
        {
        }

        protected override IQueryable<Film> Fetch(IQueryable<Film> set)
        {
            return set
                .Include(x => x.ActorFilm)
                    .ThenInclude(x => x.Actor)
                .Include(x => x.CountryFilm)
                    .ThenInclude(x => x.Country)
                .Include(x => x.GenreFilm)
                    .ThenInclude(x => x.Genre)
                .Include(x => x.ProducerFilm)
                    .ThenInclude(x => x.Producer)
                .Include(x => x.FilmCustomProperty)
                    .ThenInclude(x => x.CustomProperty);
        }

        protected override IQueryable<Film> DefaultOrder(IQueryable<Film> set)
        {
            return set.OrderBy(x => x.Name);
        }

        public async Task<List<Film>> SearchByName(string query, int limit = 100, int page = 0)
        {
            try
            {
                return await Fetch(
                        DefaultOrder(dbContext.Set<Film>())
                        .Where(x => x.Name.StartsWith(query))
                        .Skip(page * limit)
                        .Take(limit))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetAllAsync)}");
                throw;
            }
        }
    }
}
