using FilmDb;
using FilmDb.Model;
using Microsoft.EntityFrameworkCore;
using EsServices.ViewModel.Film;

namespace EsServices.Services.Consultation
{
    public class FilmRecommendationService : IFilmRecommendationService
    {
        private readonly FilmDbContext db;

        public FilmRecommendationService(FilmDbContext db)
        {
            this.db = db;
        }

        public async Task<List<FilmLookupItemVM>> GetGenresAsync(CancellationToken cancellationToken = default)
        {
            return await db.Genres
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new FilmLookupItemVM { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<FilmLookupItemVM>> GetCustomPropertiesAsync(CancellationToken cancellationToken = default)
        {
            return await db.CustomProperties
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new FilmLookupItemVM { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
        }

        public async Task<FilmResultVM?> FindByAdviceAsync(Guid adviceKey, CancellationToken cancellationToken = default)
        {
            var advice = await db.Advices
                .AsNoTracking()
                .Include(x => x.AdviceCustomProperty)
                    .ThenInclude(x => x.CustomProperty)
                .FirstOrDefaultAsync(x => x.Key == adviceKey, cancellationToken);

            if (advice == null)
                return null;

            var positiveProperties = advice.AdviceCustomProperty
                .Where(x => x.Value >= 0)
                .Select(x => x.CustomPropertyId)
                .Distinct()
                .ToList();

            if (positiveProperties.Count == 0)
                return null;

            var films = await GetFilmsWithDetails()
                .Where(x => x.FilmCustomProperty.Any(p => positiveProperties.Contains(p.CustomPropertyId)))
                .ToListAsync(cancellationToken);

            if (films.Count == 0)
                return null;

            var allProperties = await db.CustomProperties
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var adviceValues = advice.AdviceCustomProperty
                .GroupBy(x => x.CustomPropertyId)
                .ToDictionary(x => x.Key, x => x.First().Value);

            var candidates = new List<(Film Film, double Distance)>();

            foreach (var film in films)
            {
                var filmValues = film.FilmCustomProperty
                    .GroupBy(x => x.CustomPropertyId)
                    .ToDictionary(x => x.Key, x => x.First().Value);

                bool signConflict = advice.AdviceCustomProperty.Any(adviceProperty =>
                    filmValues.TryGetValue(adviceProperty.CustomPropertyId, out var filmValue) &&
                    filmValue * adviceProperty.Value < 0);

                if (signConflict)
                    continue;

                double distance = 0;
                foreach (var property in allProperties)
                {
                    var adviceValue = adviceValues.TryGetValue(property.Id, out var av) ? av : 0;
                    var filmValue = filmValues.TryGetValue(property.Id, out var fv) ? fv : 0;
                    distance += Math.Pow(adviceValue - filmValue, 2);
                }

                candidates.Add((film, distance));
            }

            if (candidates.Count == 0)
                return null;

            var topCount = Math.Max((int)(candidates.Count * 0.25), 5);
            var top = candidates
                .OrderBy(x => x.Distance)
                .ThenByDescending(x => x.Film.Rating ?? 0)
                .Take(topCount)
                .ToList();

            var selected = PickRandom(top, 5);
            return selected.Count == 0 ? null : Map(selected[Random.Shared.Next(selected.Count)].Film);
        }

        public async Task<FilmResultVM?> FindByFiltersAsync(
            IReadOnlyCollection<int> genreIds,
            IReadOnlyCollection<int> customPropertyIds,
            CancellationToken cancellationToken = default)
        {
            if (genreIds.Count == 0 && customPropertyIds.Count == 0)
                return null;

            IQueryable<Film> query = db.Films.AsNoTracking();

            foreach (var genreId in genreIds.Distinct())
                query = query.Where(x => x.GenreFilm.Any(g => g.GenreId == genreId));

            foreach (var propertyId in customPropertyIds.Distinct())
            {
                query = query.Where(x =>
                    x.FilmCustomProperty.Any(p => p.CustomPropertyId == propertyId && p.Value > 0));
            }

            var films = await WithDetails(query)
                .OrderByDescending(x => x.Rating ?? 0)
                .ToListAsync(cancellationToken);

            if (films.Count == 0)
                return null;

            var topCount = Math.Max((int)(films.Count * 0.25), 5);
            var selected = PickRandom(films.Take(topCount).ToList(), 5);

            return selected.Count == 0 ? null : Map(selected[Random.Shared.Next(selected.Count)]);
        }

        private IQueryable<Film> GetFilmsWithDetails()
        {
            return WithDetails(db.Films.AsNoTracking());
        }

        private static IQueryable<Film> WithDetails(IQueryable<Film> query)
        {
            return query
                .AsSplitQuery()
                .Include(x => x.GenreFilm)
                    .ThenInclude(x => x.Genre)
                .Include(x => x.CountryFilm)
                    .ThenInclude(x => x.Country)
                .Include(x => x.ActorFilm)
                    .ThenInclude(x => x.Actor)
                .Include(x => x.ProducerFilm)
                    .ThenInclude(x => x.Producer)
                .Include(x => x.FilmCustomProperty)
                    .ThenInclude(x => x.CustomProperty);
        }

        private static List<T> PickRandom<T>(IReadOnlyList<T> source, int count)
        {
            if (source.Count <= count)
                return source.ToList();

            return source
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .ToList();
        }

        private static FilmResultVM Map(Film film)
        {
            return new FilmResultVM
            {
                Id = film.Id,
                Name = film.Name,
                Year = film.Year,
                Rating = film.Rating ?? 0,
                Description = film.Description,
                Slogan = film.Slogan,
                Poster = film.Poster,
                Link = film.Link,
                KinopoiskId = film.KinopoiskId,
                Genres = film.GenreFilm.Select(x => x.Genre?.Name ?? string.Empty).Where(x => x != string.Empty).OrderBy(x => x).ToList(),
                Countries = film.CountryFilm.Select(x => x.Country?.Name ?? string.Empty).Where(x => x != string.Empty).OrderBy(x => x).ToList(),
                Actors = film.ActorFilm.Select(x => x.Actor?.Name ?? string.Empty).Where(x => x != string.Empty).OrderBy(x => x).ToList(),
                Producers = film.ProducerFilm.Select(x => x.Producer?.Name ?? string.Empty).Where(x => x != string.Empty).OrderBy(x => x).ToList(),
                Tags = film.FilmCustomProperty
                    .Where(x => x.CustomProperty != null)
                    .OrderByDescending(x => x.Value)
                    .Select(x => new FilmTagVM { Name = x.CustomProperty.Name, Percent = x.Value })
                    .ToList()
            };
        }
    }
}
