using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Logic
{
    public class ЗагрузкаФильмаИзКинопоиска
    {
        private DBWork db;

        public ЗагрузкаФильмаИзКинопоиска()
        {
            db = new DBWork();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; // для https
        }

        public Film ЗагрузитьПоСсылке(string url)
        {
            string html = ReadHtml(url);

            if (html.Contains("запросы, поступившие с вашего IP-адреса, похожи на автоматические"))
                throw new Exception("Кинопоиск почему-то подумал, что мы его парсим");

            Film film = ParseHtml(html);
            film.Link = url;
            return film;
        }

        public Film ЗагрузитьИзСтроки(string html)
        {
            Film film = ParseHtml(html);
            return film;
        }


        private string ReadHtml(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                    throw new Exception(response.Content.ReadAsStringAsync().Result); // текст ошибки
            }
        }

        private Film ParseHtml(string html)
        {
            Film result = new Film();

            // постер
            Regex poster = new Regex("<a class=\"popupBigImage\"(.*?)<img(.*?)src=\"(?<link>.*?)\"(.*?)/>", RegexOptions.Singleline);
            result.Poster = poster.Match(html).Groups["link"].Value;

            // название
            Regex name = new Regex("<h1 class=\"moviename-big\"(.*?)>(.*?)>(?<text>.*?)<(.*?)</h1>", RegexOptions.Singleline);
            result.Name = name.Match(html).Groups["text"].Value;

            // основная информация
            Regex infoRgx = new Regex("<table class=\"info\">(.*?)</table>", RegexOptions.Singleline);
            if (infoRgx.IsMatch(html))
                ParseMainInfo(ref result, infoRgx.Match(html).Value);

            // Актёры
            Regex actorsRgx = new Regex("<div id=\"actorList\"(.*?)<ul>(?<list>.*?)</ul>(.*?)</div>", RegexOptions.Singleline);
            if (actorsRgx.IsMatch(html))
                ParseActors(ref result, actorsRgx.Match(html).Groups["list"].Value);

            // Рейтинг 
            Regex raitingRgx = new Regex("<span class=\"rating_ball\">(?<raiting>.*?)</span>", RegexOptions.Singleline);
            result.Rating = decimal.Parse(raitingRgx.Match(html).Groups["raiting"].Value, CultureInfo.InvariantCulture);

            // Описание
            Regex descriptionRgx = new Regex("<div class=\"brand_words film-synopsys\" itemprop=\"description\">(?<desc>.*?)</div>", RegexOptions.Singleline);
            result.Description = descriptionRgx.Match(html).Groups["desc"].Value;

            return result;
        }

        private void ParseActors(ref Film film, string html)
        {
            Regex link = new Regex("<li(.*?)>(?<content>.*?)</li>", RegexOptions.Singleline);
            List<string> strList = new List<string>();

            foreach (Match match in link.Matches(html))
                strList.Add(ParseLinkList(match.Groups["content"].Value).First());

            foreach (var actorStr in strList)
            {
                if (actorStr == "...")
                    break;

                var actor = db.GetFromDatabase<Actor>(x => x.Name == actorStr).FirstOrDefault();
                if (actor == null)
                {
                    actor = new Actor()
                    {
                        Name = actorStr
                    };
                    db.Insert(actor);
                }

                film.ActorFilm.Add(new ActorFilm() { Film = film, Actor = actor });
            }
        }

        private void ParseMainInfo(ref Film film, string table)
        {
            Regex rowRgx = new Regex("<tr>(.*?)</tr>", RegexOptions.Singleline);
            Regex contentRgx = new Regex("<td(.*?)>(?<text>.*?)</td>", RegexOptions.Singleline);

            foreach (Match row in rowRgx.Matches(table))
            {
                var captionMatch = contentRgx.Matches(row.Value).Cast<Match>().First();
                var contentMatch = contentRgx.Matches(row.Value).Cast<Match>().Last();
                string caption = captionMatch.Groups["text"].Value;
                string content = contentMatch.Groups["text"].Value; // тут может быть еще теги

                switch (caption.ToLower())
                {
                    case "год":
                        List<string> years = ParseLinkList(content);
                        if (int.TryParse(years.First(), out int year))
                            film.Year = year;
                        break;
                    case "слоган":
                        film.Slogan = content;
                        break;
                    case "страна":
                        List<string> countries = ParseLinkList(content);
                        foreach (var countryStr in countries)
                        {
                            var country = db.GetFromDatabase<Country>(x => x.Name == countryStr).FirstOrDefault();
                            if (country == null)
                            {
                                country = new Country()
                                {
                                    Name = countryStr
                                };
                                db.Insert(country);
                            }

                            film.CountryFilm.Add(new CountryFilm() { Film = film, Country = country });
                        }
                        break;
                    case "режиссер":
                        List<string> producers = ParseLinkList(content);

                        foreach (var prodStr in producers)
                        {
                            if (prodStr == "...")
                                break;

                            var prod = db.GetFromDatabase<Producer>(x => x.Name == prodStr).FirstOrDefault();
                            if (prod == null)
                            {
                                prod = new Producer()
                                {
                                    Name = prodStr
                                };
                                db.Insert(prod);
                            }

                            film.ProducerFilm.Add(new ProducerFilm() { Film = film, Producer = prod});
                        }
                        break;
                    case "жанр":
                        List<string> genries = ParseLinkList(content);

                        foreach (var genreStr in genries)
                        {
                            if (genreStr == "...")
                                break;

                            var genre = db.GetFromDatabase<Genre>(x => x.Name == genreStr).FirstOrDefault();
                            if (genre == null)
                            {
                                genre = new Genre()
                                {
                                    Name = genreStr
                                };
                                db.Insert(genre);
                            }

                            film.GenreFilm.Add(new GenreFilm() { Film = film, Genre = genre });
                        }
                        break;
                }
            }
        }


        private List<string> ParseLinkList(string html)
        {
            Regex link = new Regex("<a(.*?)>(?<content>.*?)</a>", RegexOptions.Singleline);
            List<string> result = new List<string>();

            foreach (Match match in link.Matches(html))
                result.Add(match.Groups["content"].Value);

            return result;
        }


    }
}
