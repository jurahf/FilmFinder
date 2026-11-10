using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FilmsServices.Services.Kinopoisk
{
    /// <summary>
    /// Данные фильма, извлечённые из HTML-страницы Кинопоиска.
    /// </summary>
    public class KinopoiskFilmData
    {
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal? Rating { get; set; }
        public string? Slogan { get; set; }
        public string? Poster { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public string? KinopoiskId { get; set; }

        public List<string> Actors { get; } = new();
        public List<string> Countries { get; } = new();
        public List<string> Producers { get; } = new();
        public List<string> Genres { get; } = new();
    }


    /// <summary>
    /// Разбор страницы Кинопоиска. Сначала пробует старую вёрстку (той же логикой,
    /// что была в WinForms-приложении), затем безопасные fallback-и на Open Graph
    /// и JSON-LD, чтобы частичное изменение разметки не приводило к падению.
    /// </summary>
    public class KinopoiskHtmlParser
    {
        private const RegexOptions DefaultOptions =
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant;

        private static readonly Regex OldPosterRegex = new(
            "<img(?=[^>]*film-poster)[^>]*src=\"(?<link>[^\"]+)\"",
            DefaultOptions);

        private static readonly Regex OldNameRegex = new(
            "<span[^>]*class=\"[^\"]*styles_title[^\"]*\"[^>]*>(?<text>.*?)</span>",
            DefaultOptions);

        private static readonly Regex OldInfoTableRegex = new(
            "<table[^>]*class=\"[^\"]*\\binfo\\b[^\"]*\"[^>]*>(?<table>.*?)</table>",
            DefaultOptions);

        private static readonly Regex OldActorListRegex = new(
            "<div[^>]*id=\"actorList\"[^>]*>(?<body>.*?)<ul[^>]*>(?<list>.*?)</ul>",
            DefaultOptions);

        private static readonly Regex OldRatingRegex = new(
            "<span[^>]*class=\"[^\"]*rating_ball[^\"]*\"[^>]*>(?<rating>[^<]*)</span>",
            DefaultOptions);

        private static readonly Regex OldDescriptionRegex = new(
            "<div[^>]*class=\"[^\"]*film-synopsys[^\"]*\"[^>]*>(?<desc>.*?)</div>",
            DefaultOptions);

        private static readonly Regex RowRegex = new(
            "<tr[^>]*>(?<row>.*?)</tr>",
            DefaultOptions);

        private static readonly Regex CellRegex = new(
            "<td[^>]*>(?<text>.*?)</td>",
            DefaultOptions);

        private static readonly Regex LinkRegex = new(
            "<a[^>]*>(?<text>.*?)</a>",
            DefaultOptions);

        private static readonly Regex ListItemRegex = new(
            "<li[^>]*>(?<content>.*?)</li>",
            DefaultOptions);

        private static readonly Regex JsonLdRegex = new(
            "<script[^>]*type=\"application/ld\\+json\"[^>]*>(?<json>.*?)</script>",
            DefaultOptions);

        private static readonly Regex TagsRegex = new(
            "<.*?>",
            DefaultOptions);

        private static readonly Regex SpacesRegex = new(
            "\\s+",
            RegexOptions.CultureInvariant);


        public KinopoiskFilmData Parse(string html, string? url = null)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("Пустой HTML страницы Кинопоиска.", nameof(html));

            var result = new KinopoiskFilmData
            {
                Link = url,
                KinopoiskId = ExtractKinopoiskId(url)
            };

            JsonElement? jsonLd = FindJsonLdMovie(html);

            result.Name = FirstNonEmpty(
                ParseOldName(html),
                JsonString(jsonLd, "name"),
                GetMetaContent(html, "og:title"),
                ParseTitle(html));

            result.Poster = FirstNonEmpty(
                ParseOldPoster(html),
                GetMetaContent(html, "og:image"),
                JsonImage(jsonLd));

            result.Description = FirstNonEmpty(
                ParseOldDescription(html),
                GetMetaContent(html, "og:description"),
                JsonString(jsonLd, "description"));

            result.Slogan = FirstNonEmpty(
                ParseSloganFromInfoTable(html),
                JsonString(jsonLd, "slogan"));

            result.Rating = ParseOldRating(html) ?? JsonRating(jsonLd);

            result.Year = ParseOldYear(html)
                ?? JsonYear(jsonLd)
                ?? 0;

            ParseOldMainInfo(html, result);
            ParseOldActors(html, result);

            if (result.Actors.Count == 0)
                AddRange(result.Actors, JsonNames(jsonLd, "actor"));

            if (result.Producers.Count == 0)
                AddRange(result.Producers, JsonNames(jsonLd, "director"));

            if (result.Countries.Count == 0)
                AddRange(result.Countries, JsonNames(jsonLd, "countryOfOrigin"));

            if (result.Genres.Count == 0)
                AddRange(result.Genres, JsonNames(jsonLd, "genre"));

            return result;
        }


        /// <summary>
        /// Достаёт id фильма из адреса вида /film/12345/ или ?film_id=12345.
        /// </summary>
        public static string? ExtractKinopoiskId(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            Match match = Regex.Match(url, @"/(?:film|series)/(?<id>\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups["id"].Value;

            match = Regex.Match(url, @"[?&]film_id=(?<id>\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups["id"].Value;

            return null;
        }


        private static string? ParseOldName(string html)
        {
            Match match = OldNameRegex.Match(html);
            return match.Success ? Clean(match.Groups["text"].Value) : null;
        }


        private static string? ParseOldPoster(string html)
        {
            Match match = OldPosterRegex.Match(html);
            return match.Success ? Clean(match.Groups["link"].Value) : null;
        }


        private static string? ParseOldDescription(string html)
        {
            Match match = OldDescriptionRegex.Match(html);
            return match.Success ? Clean(match.Groups["desc"].Value) : null;
        }


        private static decimal? ParseOldRating(string html)
        {
            Match match = OldRatingRegex.Match(html);

            if (match.Success && decimal.TryParse(
                    Clean(match.Groups["rating"].Value),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal rating))
                return rating;

            return null;
        }


        private static int? ParseOldYear(string html)
        {
            Match table = OldInfoTableRegex.Match(html);
            if (!table.Success)
                return null;

            foreach (Match row in RowRegex.Matches(table.Groups["table"].Value))
            {
                List<string> cells = CellRegex.Matches(row.Groups["row"].Value)
                    .Cast<Match>()
                    .Select(x => Clean(x.Groups["text"].Value))
                    .ToList();

                if (cells.Count < 2 || !IsCaption(cells[0], "год"))
                    continue;

                Match year = Regex.Match(cells[1], @"\b(18|19|20)\d{2}\b");
                if (year.Success && int.TryParse(year.Value, out int value))
                    return value;
            }

            return null;
        }


        private static string? ParseSloganFromInfoTable(string html)
        {
            Match table = OldInfoTableRegex.Match(html);
            if (!table.Success)
                return null;

            foreach (Match row in RowRegex.Matches(table.Groups["table"].Value))
            {
                List<string> cells = CellRegex.Matches(row.Groups["row"].Value)
                    .Cast<Match>()
                    .Select(x => Clean(x.Groups["text"].Value))
                    .ToList();

                if (cells.Count >= 2 && IsCaption(cells[0], "слоган"))
                    return NullIfEmpty(cells[1]);
            }

            return null;
        }


        private static void ParseOldMainInfo(string html, KinopoiskFilmData result)
        {
            Match table = OldInfoTableRegex.Match(html);
            if (!table.Success)
                return;

            foreach (Match row in RowRegex.Matches(table.Groups["table"].Value))
            {
                MatchCollection cellMatches = CellRegex.Matches(row.Groups["row"].Value);
                if (cellMatches.Count < 2)
                    continue;

                string caption = Clean(cellMatches[0].Groups["text"].Value);
                string content = cellMatches[^1].Groups["text"].Value;

                if (IsCaption(caption, "страна"))
                    AddRange(result.Countries, ParseLinks(content));
                else if (IsCaption(caption, "режиссер") || IsCaption(caption, "режиссёр"))
                    AddRange(result.Producers, ParseLinks(content));
                else if (IsCaption(caption, "жанр"))
                    AddRange(result.Genres, ParseLinks(content));
                else if (IsCaption(caption, "слоган"))
                    result.Slogan ??= NullIfEmpty(Clean(content));
            }
        }


        private static void ParseOldActors(string html, KinopoiskFilmData result)
        {
            Match block = OldActorListRegex.Match(html);
            if (!block.Success)
                return;

            foreach (Match item in ListItemRegex.Matches(block.Groups["list"].Value))
            {
                string content = item.Groups["content"].Value;
                string name = ParseLinks(content).FirstOrDefault() ?? Clean(content);

                if (name == "...")
                    break;

                AddUnique(result.Actors, name);
            }
        }


        private static List<string> ParseLinks(string html)
        {
            return LinkRegex.Matches(html)
                .Cast<Match>()
                .Select(x => Clean(x.Groups["text"].Value))
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "...")
                .ToList();
        }


        private static string? ParseTitle(string html)
        {
            Match match = Regex.Match(html, "<title[^>]*>(?<title>.*?)</title>", DefaultOptions);
            if (!match.Success)
                return null;

            string title = Clean(match.Groups["title"].Value);
            title = Regex.Replace(title, @"\s*[-—|]\s*Кинопоиск.*$", "", RegexOptions.IgnoreCase);
            return NullIfEmpty(title);
        }


        private static string? GetMetaContent(string html, string property)
        {
            string escaped = Regex.Escape(property);
            string[] patterns =
            {
                $@"<meta[^>]*(?:property|name)\s*=\s*[""']{escaped}[""'][^>]*content\s*=\s*[""'](?<value>[^""']*)[""']",
                $@"<meta[^>]*content\s*=\s*[""'](?<value>[^""']*)[""'][^>]*(?:property|name)\s*=\s*[""']{escaped}[""']"
            };

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(html, pattern, DefaultOptions);
                if (match.Success)
                    return NullIfEmpty(Clean(match.Groups["value"].Value));
            }

            return null;
        }


        private static JsonElement? FindJsonLdMovie(string html)
        {
            foreach (Match match in JsonLdRegex.Matches(html))
            {
                try
                {
                    using JsonDocument document = JsonDocument.Parse(match.Groups["json"].Value);
                    JsonElement? movie = FindMovieElement(document.RootElement);

                    if (movie.HasValue)
                        return movie.Value.Clone();
                }
                catch (JsonException)
                {
                    // Один битый блок JSON-LD не должен ломать разбор страницы.
                }
            }

            return null;
        }


        private static JsonElement? FindMovieElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (HasMovieType(element))
                    return element;

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    JsonElement? found = FindMovieElement(property.Value);
                    if (found.HasValue)
                        return found;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    JsonElement? found = FindMovieElement(item);
                    if (found.HasValue)
                        return found;
                }
            }

            return null;
        }


        private static bool HasMovieType(JsonElement element)
        {
            if (!element.TryGetProperty("@type", out JsonElement type))
                return false;

            if (type.ValueKind == JsonValueKind.String)
                return IsMovieType(type.GetString());

            if (type.ValueKind == JsonValueKind.Array)
                return type.EnumerateArray()
                    .Any(x => x.ValueKind == JsonValueKind.String && IsMovieType(x.GetString()));

            return false;
        }


        private static bool IsMovieType(string? type)
        {
            return type != null
                && (type.Contains("Movie", StringComparison.OrdinalIgnoreCase)
                    || type.Contains("TVSeries", StringComparison.OrdinalIgnoreCase)
                    || type.Contains("TVMovie", StringComparison.OrdinalIgnoreCase));
        }


        private static string? JsonString(JsonElement? element, string property)
        {
            if (!element.HasValue
                || element.Value.ValueKind != JsonValueKind.Object
                || !element.Value.TryGetProperty(property, out JsonElement value))
                return null;

            return value.ValueKind switch
            {
                JsonValueKind.String => NullIfEmpty(Clean(value.GetString())),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }


        private static string? JsonImage(JsonElement? element)
        {
            if (!element.HasValue
                || element.Value.ValueKind != JsonValueKind.Object
                || !element.Value.TryGetProperty("image", out JsonElement image))
                return null;

            if (image.ValueKind == JsonValueKind.String)
                return NullIfEmpty(Clean(image.GetString()));

            if (image.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in image.EnumerateArray())
                {
                    string? value = JsonImageValue(item);
                    if (value != null)
                        return value;
                }
            }

            return JsonImageValue(image);
        }


        private static string? JsonImageValue(JsonElement image)
        {
            if (image.ValueKind == JsonValueKind.String)
                return NullIfEmpty(Clean(image.GetString()));

            if (image.ValueKind == JsonValueKind.Object
                && image.TryGetProperty("url", out JsonElement url)
                && url.ValueKind == JsonValueKind.String)
                return NullIfEmpty(Clean(url.GetString()));

            return null;
        }


        private static decimal? JsonRating(JsonElement? element)
        {
            if (!element.HasValue
                || element.Value.ValueKind != JsonValueKind.Object
                || !element.Value.TryGetProperty("aggregateRating", out JsonElement rating)
                || rating.ValueKind != JsonValueKind.Object
                || !rating.TryGetProperty("ratingValue", out JsonElement value))
                return null;

            string? text = value.ValueKind switch
            {
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.String => value.GetString(),
                _ => null
            };

            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result)
                ? result
                : null;
        }


        private static int? JsonYear(JsonElement? element)
        {
            string? published = JsonString(element, "datePublished");
            if (published == null)
                return null;

            Match match = Regex.Match(published, @"\b(18|19|20)\d{2}\b");
            return match.Success && int.TryParse(match.Value, out int year) ? year : null;
        }


        private static List<string> JsonNames(JsonElement? element, string property)
        {
            var result = new List<string>();

            if (!element.HasValue
                || element.Value.ValueKind != JsonValueKind.Object
                || !element.Value.TryGetProperty(property, out JsonElement value))
                return result;

            if (value.ValueKind == JsonValueKind.String)
            {
                AddUnique(result, Clean(value.GetString()));
            }
            else if (value.ValueKind == JsonValueKind.Object)
            {
                AddUnique(result, JsonString(value, "name"));
            }
            else if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                        AddUnique(result, Clean(item.GetString()));
                    else if (item.ValueKind == JsonValueKind.Object)
                        AddUnique(result, JsonString(item, "name"));
                }
            }

            return result;
        }


        private static bool IsCaption(string caption, string expected)
        {
            return string.Equals(
                caption.Trim().TrimEnd(':').Trim(),
                expected,
                StringComparison.OrdinalIgnoreCase);
        }


        private static string? FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }


        private static void AddRange(List<string> target, IEnumerable<string> values)
        {
            foreach (string value in values)
                AddUnique(target, value);
        }


        private static void AddUnique(List<string> target, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            string cleaned = Clean(value);

            if (cleaned.Length == 0 || cleaned == "...")
                return;

            if (!target.Any(x => string.Equals(x, cleaned, StringComparison.OrdinalIgnoreCase)))
                target.Add(cleaned);
        }


        private static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }


        private static string Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string withoutTags = TagsRegex.Replace(value, " ");
            string decoded = WebUtility.HtmlDecode(withoutTags);
            return SpacesRegex.Replace(decoded, " ").Trim();
        }
    }
}
