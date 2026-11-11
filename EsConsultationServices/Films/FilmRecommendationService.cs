using FilmDb;
using FilmDb.Model;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationServices.Films;

/// <summary>
/// Подбор фильма по совету экспертной системы.
/// Сначала возвращаются явно указанные в совете фильмы (AdviceFilm),
/// затем — похожие по вектору свойств фильмы.
/// </summary>
public sealed class FilmRecommendationService : IFilmRecommendationService
{
    private readonly FilmDbContext db;

    public FilmRecommendationService(FilmDbContext db)
    {
        this.db = db;
    }

    public async Task<RecommendedFilm?> FindByAdviceAsync(
        Guid adviceKey,
        IReadOnlyCollection<int>? excludedFilmIds = null,
        CancellationToken cancellationToken = default)
    {
        var excluded = ToSet(excludedFilmIds);

        var advice = await db.Advices
            .AsNoTracking()
            .Include(x => x.AdviceCustomProperty)
                .ThenInclude(x => x.CustomProperty)
            .Include(x => x.AdviceFilm)
                .ThenInclude(x => x.Film)
            .FirstOrDefaultAsync(x => x.Key == adviceKey, cancellationToken);

        if (advice == null)
            return null;

        // 1. Явные фильмы совета — основные результаты.
        var explicitFilm = advice.AdviceFilm
            .Where(x => x.Film != null && !excluded.Contains(x.FilmId))
            .OrderByDescending(x => x.Value)
            .ThenByDescending(x => x.Film.Rating ?? 0)
            .ThenBy(x => x.FilmId)
            .FirstOrDefault();

        if (explicitFilm != null)
        {
            var film = await GetFilmWithDetailsAsync(explicitFilm.FilmId, cancellationToken);
            if (film != null)
                return Map(film);
        }

        // 2. Похожие фильмы по вектору свойств — когда явные закончились.
        return await FindSimilarAsync(advice, excluded, cancellationToken);
    }

    public async Task<RecommendedFilm?> FindByFiltersAsync(
        IReadOnlyCollection<int> genreIds,
        IReadOnlyCollection<int> customPropertyIds,
        IReadOnlyCollection<int>? excludedFilmIds = null,
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

        var excluded = excludedFilmIds?.ToHashSet();
        if (excluded is { Count: > 0 })
            query = query.Where(x => !excluded.Contains(x.Id));

        var films = await query
            .Select(x => new { x.Id, x.Rating })
            .OrderByDescending(x => x.Rating)
            .ToListAsync(cancellationToken);

        if (films.Count == 0)
            return null;

        var topCount = Math.Max((int)(films.Count * 0.25), 5);
        var selected = PickRandom(films.Take(topCount).ToList(), 5);
        if (selected.Count == 0)
            return null;

        var chosenId = selected[Random.Shared.Next(selected.Count)].Id;
        var film = await GetFilmWithDetailsAsync(chosenId, cancellationToken);
        return film == null ? null : Map(film);
    }

    private async Task<RecommendedFilm?> FindSimilarAsync(
        Advice advice,
        IReadOnlySet<int> excluded,
        CancellationToken cancellationToken)
    {
        var positiveProperties = advice.AdviceCustomProperty
            .Where(x => x.Value >= 0)
            .Select(x => x.CustomPropertyId)
            .Distinct()
            .ToList();

        if (positiveProperties.Count == 0)
            return null;

        // Для расчёта расстояний достаточно значений свойств и рейтинга,
        // поэтому не вычитываем фильмы с актёрами/жанрами целиком.
        var propertyRows = await db.FilmCustomProperties
            .AsNoTracking()
            .Where(x => positiveProperties.Contains(x.CustomPropertyId))
            .Select(x => new
            {
                FilmId = x.Film.Id,
                x.CustomPropertyId,
                x.Value
            })
            .ToListAsync(cancellationToken);

        if (propertyRows.Count == 0)
            return null;

        var allProperties = await db.CustomProperties
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var filmProperties = propertyRows
            .GroupBy(x => x.FilmId)
            .Where(x => !excluded.Contains(x.Key))
            .ToDictionary(
                x => x.Key,
                x => x.GroupBy(p => p.CustomPropertyId)
                    .ToDictionary(p => p.Key, p => p.First().Value));

        if (filmProperties.Count == 0)
            return null;

        var filmIds = filmProperties.Keys.ToList();
        var ratings = await db.Films
            .AsNoTracking()
            .Where(x => filmIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Rating })
            .ToDictionaryAsync(x => x.Id, x => x.Rating ?? 0, cancellationToken);

        var adviceValues = advice.AdviceCustomProperty
            .GroupBy(x => x.CustomPropertyId)
            .ToDictionary(x => x.Key, x => x.First().Value);

        var candidates = new List<(int FilmId, double Distance, decimal Rating)>();

        foreach (var pair in filmProperties)
        {
            var filmValues = pair.Value;

            bool signConflict = advice.AdviceCustomProperty.Any(adviceProperty =>
                filmValues.TryGetValue(adviceProperty.CustomPropertyId, out var filmValue) &&
                filmValue * adviceProperty.Value < 0);

            if (signConflict)
                continue;

            double distance = 0;
            foreach (var propertyId in allProperties)
            {
                var adviceValue = adviceValues.TryGetValue(propertyId, out var av) ? av : 0;
                var filmValue = filmValues.TryGetValue(propertyId, out var fv) ? fv : 0;
                distance += Math.Pow(adviceValue - filmValue, 2);
            }

            candidates.Add((pair.Key, distance, ratings.TryGetValue(pair.Key, out var rating) ? rating : 0));
        }

        if (candidates.Count == 0)
            return null;

        var topCount = Math.Max((int)(candidates.Count * 0.25), 5);
        var top = candidates
            .OrderBy(x => x.Distance)
            .ThenByDescending(x => x.Rating)
            .Take(topCount)
            .ToList();

        var selected = PickRandom(top, 5);
        if (selected.Count == 0)
            return null;

        var chosenId = selected[Random.Shared.Next(selected.Count)].FilmId;
        var film = await GetFilmWithDetailsAsync(chosenId, cancellationToken);
        return film == null ? null : Map(film);
    }

    private async Task<Film?> GetFilmWithDetailsAsync(int filmId, CancellationToken cancellationToken)
    {
        return await WithDetails(db.Films.AsNoTracking().Where(x => x.Id == filmId))
            .FirstOrDefaultAsync(cancellationToken);
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

    private static HashSet<int> ToSet(IReadOnlyCollection<int>? source)
    {
        return source == null ? new HashSet<int>() : new HashSet<int>(source);
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

    private static RecommendedFilm Map(Film film)
    {
        return new RecommendedFilm
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
                .Select(x => new FilmTag { Name = x.CustomProperty.Name, Percent = x.Value })
                .ToList()
        };
    }
}
