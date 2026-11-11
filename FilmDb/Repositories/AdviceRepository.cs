using CommonRepositories;
using FilmDb.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FilmDb.Repositories
{
    public interface IAdviceRepository : IRepository<Advice>
    {
        /// <summary>
        /// Описания советов: ключ совета (GUID в формате "D") -> человекочитаемый текст.
        /// Единое построение описания для UI и API.
        /// </summary>
        Task<IReadOnlyDictionary<string, string>> GetDescriptionsAsync(CancellationToken cancellationToken = default);
    }


    public class AdviceRepository : BaseRepository<Advice>, IAdviceRepository, IRepository<Advice>
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


        protected override IQueryable<Advice> DefaultOrder(IQueryable<Advice> set)
        {
            return set.OrderBy(x => x.Key);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetDescriptionsAsync(CancellationToken cancellationToken = default)
        {
            var advices = await Fetch(dbContext.Set<Advice>().AsNoTracking())
                .ToListAsync(cancellationToken);

            return advices.ToDictionary(
                x => x.Key.ToString("D"),
                BuildDescription,
                StringComparer.OrdinalIgnoreCase);
        }

        private static string BuildDescription(Advice advice)
        {
            var sb = new StringBuilder();

            foreach (var adviceFilm in advice.AdviceFilm)
            {
                if (!string.IsNullOrWhiteSpace(adviceFilm.Film?.Name))
                    sb.Append(adviceFilm.Film.Name).Append("; ");
            }

            foreach (var property in advice.AdviceCustomProperty)
            {
                var name = property.CustomProperty?.Name ?? $"свойство {property.CustomPropertyId}";
                var no = property.Value < 0 ? "НЕ " : string.Empty;
                sb.Append($"{no}{name} ({property.Value}); ");
            }

            var result = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? $"Совет {advice.Key}" : result;
        }
    }
}
