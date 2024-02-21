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
    public class AdviceRepository : BaseRepository<Advice>, IRepository<Advice>
    {
        public AdviceRepository(FilmDbContext dbContext) 
            : base(dbContext)
        {
        }

        protected override IQueryable<Advice> Fetch(IQueryable<Advice> set)
        {
            return set
                .Include(x => x.AdviceCustomProperty)
                    .ThenInclude(x => x.CustomProperty)
                .Include(x => x.AdviceFilm)
                    .ThenInclude(x => x.Film);
        }
    }
}
