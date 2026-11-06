using FilmDb;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EsServices.Services.Advice
{
    public class AdviceCatalogService : IAdviceCatalogService
    {
        private readonly FilmDbContext db;

        public AdviceCatalogService(FilmDbContext db)
        {
            this.db = db;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetDescriptionsAsync(CancellationToken cancellationToken = default)
        {
            var advices = await db.Advices
                .AsNoTracking()
                .Include(x => x.AdviceCustomProperty)
                    .ThenInclude(x => x.CustomProperty)
                .Include(x => x.AdviceFilm)
                    .ThenInclude(x => x.Film)
                .ToListAsync(cancellationToken);

            return advices.ToDictionary(
                x => x.Key.ToString("D"),
                BuildDescription,
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetDescriptionsByKeyAsync(CancellationToken cancellationToken = default)
        {
            return await GetDescriptionsAsync(cancellationToken);
        }

        private static string BuildDescription(FilmDb.Model.Advice advice)
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
