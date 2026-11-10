using FilmDb;
using FilmDb.Model;
using FilmsServices.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FilmsServices.Services.Kinopoisk
{
    public interface IKinopoiskFilmImportService
    {
        /// <summary>
        /// Загружает фильм по ссылке на Кинопоиск и возвращает Id сохранённого фильма.
        /// </summary>
        Task<int> ImportByUrlAsync(string url, CancellationToken cancellationToken = default);
    }


    public class KinopoiskFilmImportService : IKinopoiskFilmImportService
    {
        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

        private static readonly string[] BlockMarkers =
        {
            "похожи на автоматические",
            "sso.kinopoisk.ru",
            "Подтвердите, что запросы отправляли вы",
            "Are you a robot"
        };

        private readonly HttpClient httpClient;
        private readonly FilmDbContext dbContext;
        private readonly KinopoiskHtmlParser parser;


        public KinopoiskFilmImportService(
            HttpClient httpClient,
            FilmDbContext dbContext,
            KinopoiskHtmlParser parser)
        {
            this.httpClient = httpClient;
            this.dbContext = dbContext;
            this.parser = parser;
        }


        public async Task<int> ImportByUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            Uri uri = ValidateUrl(url);
            string html = await DownloadHtmlAsync(uri, cancellationToken);
            KinopoiskFilmData data = parser.Parse(html, uri.ToString());

            if (string.IsNullOrWhiteSpace(data.Name)
                || (data.Year == 0 && data.Actors.Count == 0 && data.Genres.Count == 0))
                throw new InvalidOperationException(
                    "Не удалось распознать страницу фильма. Возможно, Кинопоиск изменил вёрстку.");

            Film? existing = await dbContext.Films
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == data.Name && x.Year == data.Year, cancellationToken);

            if (existing != null)
                throw new FilmAlreadyExistsException(existing.Id, data.Name, data.Year);

            var film = new Film
            {
                Name = data.Name,
                Year = data.Year,
                Rating = data.Rating,
                Slogan = data.Slogan,
                Poster = data.Poster,
                Description = data.Description,
                Link = data.Link,
                KinopoiskId = data.KinopoiskId
            };

            foreach (string name in data.Actors)
                film.ActorFilm.Add(new ActorFilm() { Film = film, Actor = await GetOrCreateActorAsync(name, cancellationToken) });

            foreach (string name in data.Countries)
                film.CountryFilm.Add(new CountryFilm() { Film = film, Country = await GetOrCreateCountryAsync(name, cancellationToken) });

            foreach (string name in data.Producers)
                film.ProducerFilm.Add(new ProducerFilm() { Film = film, Producer = await GetOrCreateProducerAsync(name, cancellationToken) });

            foreach (string name in data.Genres)
                film.GenreFilm.Add(new GenreFilm() { Film = film, Genre = await GetOrCreateGenreAsync(name, cancellationToken) });

            dbContext.Films.Add(film);
            await dbContext.SaveChangesAsync(cancellationToken);

            return film.Id;
        }


        private static Uri ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Укажите ссылку на фильм Кинопоиска.");

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Ссылка должна быть полным адресом вида https://www.kinopoisk.ru/film/...");

            if (!string.Equals(uri.Host, "kinopoisk.ru", StringComparison.OrdinalIgnoreCase)
                && !uri.Host.EndsWith(".kinopoisk.ru", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Поддерживаются только ссылки на kinopoisk.ru.");

            return uri;
        }


        private async Task<string> DownloadHtmlAsync(Uri uri, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            request.Headers.TryAddWithoutValidation(
                "Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            request.Headers.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
            request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
            request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

            using HttpResponseMessage response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
                || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new KinopoiskBlockedException();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Кинопоиск ответил кодом {(int)response.StatusCode} {response.ReasonPhrase}.");

            string html = await response.Content.ReadAsStringAsync(cancellationToken);

            if (IsBlockedPage(html))
                throw new KinopoiskBlockedException();

            return html;
        }


        private static bool IsBlockedPage(string html)
        {
            return BlockMarkers.Any(marker =>
                html.Contains(marker, StringComparison.OrdinalIgnoreCase));
        }


        private async Task<Actor> GetOrCreateActorAsync(string name, CancellationToken cancellationToken)
        {
            Actor? entity = await dbContext.Actors
                .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

            if (entity != null)
                return entity;

            entity = new Actor() { Name = name };
            dbContext.Actors.Add(entity);
            return entity;
        }


        private async Task<Country> GetOrCreateCountryAsync(string name, CancellationToken cancellationToken)
        {
            Country? entity = await dbContext.Countries
                .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

            if (entity != null)
                return entity;

            entity = new Country() { Name = name };
            dbContext.Countries.Add(entity);
            return entity;
        }


        private async Task<Producer> GetOrCreateProducerAsync(string name, CancellationToken cancellationToken)
        {
            Producer? entity = await dbContext.Producers
                .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

            if (entity != null)
                return entity;

            entity = new Producer() { Name = name };
            dbContext.Producers.Add(entity);
            return entity;
        }


        private async Task<Genre> GetOrCreateGenreAsync(string name, CancellationToken cancellationToken)
        {
            Genre? entity = await dbContext.Genres
                .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

            if (entity != null)
                return entity;

            entity = new Genre() { Name = name };
            dbContext.Genres.Add(entity);
            return entity;
        }
    }
}
