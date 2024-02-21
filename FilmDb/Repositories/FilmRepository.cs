using CommonRepositories;
using FilmDb.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Repositories
{
    public class FilmRepository : BaseRepository<Film>, IRepository<Film>
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

    }
}
