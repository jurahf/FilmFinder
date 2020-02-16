using ExpertSystemDb;
using Logic.Exceptions;
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
            CheckExists(film, true);
            db.Insert(film);

            return film;
        }

        public Film ЗагрузитьИзСтроки(string html)
        {
            Film film = ParseHtml(html);

            CheckExists(film, true);
            db.Insert(film);

            return film;
        }


        private bool CheckExists(Film film, bool throwException)
        {
            bool result = db.GetFromDatabase<Film>().Any(x => x.Name == film.Name && x.Year == film.Year);

            if (result && throwException)
                throw new FilmAlreadyExistsException();

            return result;
        }

        private string ReadHtml(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Add("Accept", "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("cache-control", "no-cache");
                //client.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");
                client.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
                client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36 OPR/65.0.3467.56");
                client.DefaultRequestHeaders.Add("cookie", "_ym_uid=150541113972722960; my_perpages=%5B%5D; yandexuid=6414368961517245150; fuid01=5a78815c7a08051d.MFL9DR5i78P--Z7PRVNDZ3E2f7IG-IoD3IFEgYU1rU_ObD7S73uuB7p6xozHFi7UIWavKnGMW94LE962v0Uk_uYaMF8dhlnB0o5Bh2LgsfVJqtuJ4p5BzNx3ZqRMOmv8; deadpool_dialog_previous_teaser_group=1; mda=0; mda_exp_enabled=1; yuidss=6414368961517245150; _ym_uid=150541113972722960; _ym_d=1575206966; i=Z0WNOwIJE4T8sChin9WWbh9fPjKm/HRIh21TS7C66YexocccOZxJTC/7agQLPlDagzT5yS59SO630OqxsajZB4GT0GI=; mobile=no; tc=432; _ym_wasSynced=%7B%22time%22%3A1580483854536%2C%22params%22%3A%7B%22eu%22%3A0%7D%2C%22bkParams%22%3A%7B%7D%7D; _ym_isad=2; yp=1583075854.oyu.6414368961517245150#1580570254.yu.6414368961517245150; PHPSESSID=9c21odq6g158ejipp27uagd467; user_country=ru; yandex_gid=50; _csrf_csrf_token=dNTMK5iPVKVMG-g--tEv_5Pyy_OVnjIQSJaCpryvnXU; desktop_session_key=a5dd725daccd54bc2967328af43121b7cffa05f5ae3d75fb1787cd82ce3769a844a9fd3e7c7c9cdaded0d07c6193a5b1df829204b8083ac7afee7db4c05ab5c7d45ec136a88b2607f4051a0c2758f9437adf429387986b966173c57142bc28bd; desktop_session_key.sig=p5ldZwSQwP7Ox1ntxwkebhTV4yQ; _ym_d=1580545806; yandex_plus_metrika_cookie=true; _ym_visorc_22663942=b; _ym_visorc_52332406=b; _ym_visorc_56177992=b; ya_sess_id=noauth:1580545806; ys=c_chck.3764512617; mda2_beacon=1580545806749; sso_status=sso.passport.yandex.ru:synchronized; cycada=SaKHUcx+9GRmhpDjTxTrQuyn0Ts8PXNEkqyKLp+tZtk=");

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
