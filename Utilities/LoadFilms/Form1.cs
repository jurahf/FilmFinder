using ExpertSystemDb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetOffice;
using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using System.Globalization;

namespace LoadFilms
{
    public partial class Form1 : Form
    {
        IDataWork db = null;

        public Form1()
        {
            InitializeComponent();

            db = new DBWork();
        }


        /// <summary>
        /// первоначальная прогрузка из movies.csv
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Regex regex = new Regex(@"^(?<id>\d*),(?<title>.*?)\((?<year>\d\d\d\d)\).*?,(?<genre>[\w-()|\s;]*?)$");
                List<IMDbLoading> result = new List<IMDbLoading>();

                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    sr.ReadLine(); // первую строку пропускаем
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        Match match = regex.Match(line);

                        IMDbLoading imdb = new IMDbLoading()
                        {
                            DataSetID = match.Groups["id"].Value,
                            EnglishTitle = match.Groups["title"].Value,
                            Year = match.Groups["year"].Value,
                            EnglishGenries = match.Groups["genre"].Value,
                        };

                        result.Add(imdb);
                    }
                }


                db.Insert(result);
            }
        }


        /// <summary>
        /// тэги и количество упоминаний из tags.csv
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    sr.ReadLine(); // первую строку пропускаем
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] parts = line.Split(',');
                        string tag = parts[2].ToLower();
                        if (!result.ContainsKey(tag))
                            result.Add(tag, 1);
                        else
                            result[tag]++;
                    }
                }

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        foreach (var res in result.Where(x => x.Value > 100).OrderBy(x => x.Key))
                        {
                            sw.WriteLine($"{res.Key}");
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Загрузка ссылок на IMDb
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Regex regex = new Regex(@"^(?<id>\d*),(?<imdbId>\d*),(?<tmdbId>\d*)$");
                List<IMDbLoading> result = new List<IMDbLoading>();

                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    sr.ReadLine(); // первую строку пропускаем
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        Match match = regex.Match(line);
                        string dataSetId = match.Groups["id"].Value;
                        string imdbId = match.Groups["imdbId"].Value;
                        //string tbdbId 

                        // TODO: это слишком долго
                        IMDbLoading imdb = db.GetFromDatabase<IMDbLoading>(x => x.DataSetID == dataSetId).FirstOrDefault();
                        if (imdb != null)
                        {
                            imdb.IMDbId = imdbId;
                            result.Add(imdb);
                        }
                    }
                }


                db.Update(result);
            }
        }


        /// <summary>
        /// Присвоить русские названия
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Regex regex = new Regex(@"^tt(?<imdbId>\d*)\t(?<title>.*?)$");
                List<IMDbLoading> result = db.GetFromDatabase<IMDbLoading>();

                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        Match match = regex.Match(line);
                        string imdbId = match.Groups["imdbId"].Value;
                        string title = match.Groups["title"].Value;

                        var imdb = result.FirstOrDefault(x => x.IMDbId == imdbId);
                        if (imdb != null)
                        {
                            imdb.RussianTitle = title;
                        }
                    }
                }


                db.Update(result);
            }
        }



        /// <summary>
        /// Найти и присвоить тэги
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Regex regex = new Regex(@"^(?<userId>\d*),(?<filmId>\d*),(?<tag>.*?),(?<timestamp>\d*)$");
                List<IMDbLoading> result = db.GetFromDatabase<IMDbLoading>();
                Dictionary<string, Dictionary<string, int>> filmTagCount = new Dictionary<string, Dictionary<string, int>>();

                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    sr.ReadLine(); // первую строку пропускаем
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        Match match = regex.Match(line);
                        string dataSetId = match.Groups["filmId"].Value;
                        string tag = match.Groups["tag"].Value.Trim().ToLower();

                        if (!filmTagCount.ContainsKey(dataSetId))
                            filmTagCount.Add(dataSetId, new Dictionary<string, int>());

                        if (!filmTagCount[dataSetId].ContainsKey(tag))
                            filmTagCount[dataSetId].Add(tag, 0);

                        filmTagCount[dataSetId][tag]++;
                    }
                }

                foreach (var filmTag in filmTagCount)
                {
                    var film = result.FirstOrDefault(x => x.DataSetID == filmTag.Key);

                    if (film != null)
                    {
                        film.EnglishTags = string.Join("|", filmTag.Value.Select(p => $"{p.Key}({p.Value})")).Trim('|');
                    }
                }

                db.Update(result);
            }
        }

        /// <summary>
        /// Запрашиваем из API
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            List<IMDbLoading> searchList = db.GetFromDatabase<IMDbLoading>()
                //.Where(x => !string.IsNullOrEmpty(x.RussianTitle))
                .Where(x => string.IsNullOrEmpty(x.Poster))
                .ToList();

            string url = "https://movie-database-imdb-alternative.p.rapidapi.com/";
            string r = "json";
            string plot = "full";
            string rapidapiKey = "a88da764d3mshb08efa65108611dp18acc7jsn87e8b23fc6ba";
            string rapidapiHost = "movie-database-imdb-alternative.p.rapidapi.com";

            int limit = 1000;
            int count = 0;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-rapidapi-host", rapidapiHost);
                    client.DefaultRequestHeaders.Add("x-rapidapi-key", rapidapiKey);

                    foreach (var film in searchList)
                    {
                        string i = $"tt{film.IMDbId}";

                        var response = client.GetAsync($"{url}?r={r}&i={i}&plot={plot}").Result;
                        count++;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonContent = response.Content.ReadAsStringAsync().Result;
                            ImdbApiResponse parsed = JsonConvert.DeserializeObject<ImdbApiResponse>(jsonContent);

                            film.EnglishActors = parsed.Actors;
                            film.EnglishCountries = parsed.Country;
                            film.EnglishProducers = parsed.Director;
                            film.Poster = parsed.Poster;
                            film.EnglishDescription = parsed.Plot;
                            film.Rating = parsed.imdbRating; // parsed.Ratings.FirstOrDefault(x => x.Source == "Internet Movie Database")?.Value;

                            // можно еще обновить жанр и английское название
                        }

                        if (count > limit)
                        {
                            count = 0;
                            db.Update(searchList);
                        }
                    }
                }
            }
            finally
            {
                db.Update(searchList);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<IMDbLoading> searchList = db.GetFromDatabase<IMDbLoading>()
                .Where(x => !string.IsNullOrEmpty(x.RussianTitle))
                .Where(x => string.IsNullOrEmpty(x.RussianDescription))
                .Where(x => !string.IsNullOrEmpty(x.EnglishDescription))
                .ToList();

            string url = "https://kiaratranslate.p.rapidapi.com/get_translated/";
            string rapidapiKey = "a88da764d3mshb08efa65108611dp18acc7jsn87e8b23fc6ba";
            string rapidapiHost = "kiaratranslate.p.rapidapi.com";

            int limit = 1000;
            int count = 0;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-rapidapi-host", rapidapiHost);
                    client.DefaultRequestHeaders.Add("x-rapidapi-key", rapidapiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    foreach (var film in searchList)
                    {
                        string textToTranslate = Regex.Replace(film.EnglishDescription, @"\sor\s", " или "); // костыль, потому что сервис не может перевести "or"

                        TranslateApiParams payload = new TranslateApiParams(textToTranslate);
                        string stringPayload = JsonConvert.SerializeObject(payload);
                        HttpContent content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                        request.Content = content;
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.SendAsync(request).Result;
                        count++;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonContent = response.Content.ReadAsStringAsync().Result;
                            TranslateApiResponse parsed = JsonConvert.DeserializeObject<TranslateApiResponse>(jsonContent);

                            film.RussianDescription = parsed.Translated;
                        }

                        if (count > limit)
                        {
                            count = 0;
                            db.Update(searchList);
                        }
                    }
                }
            }
            finally
            {
                db.Update(searchList);
            }
        }




        private void button8_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var translates = GetTranslatesFromExcel(openFileDialog1.FileName);

                List<IMDbLoading> allFilms = db.GetFromDatabase<IMDbLoading>()
                    .Where(x => !string.IsNullOrEmpty(x.EnglishTags))
                    .Where(x => string.IsNullOrEmpty(x.RussianTags))
                    .ToList();

                Regex reg = new Regex(@"(?<tag>.*)\((?<count>\d*)\)$");

                foreach (var film in allFilms)
                {
                    string[] tags = film.EnglishTags.Split('|');
                    Dictionary<string, int> newTags = new Dictionary<string, int>();

                    foreach (var tag in tags)
                    {
                        var match = reg.Match(tag);
                        string engTag = match.Groups["tag"].Value;
                        int count = int.Parse(match.Groups["count"].Value);

                        if (translates.ContainsKey(engTag))
                        {
                            foreach (var rusTag in translates[engTag])
                            {
                                if (!newTags.ContainsKey(rusTag))
                                    newTags.Add(rusTag, 0);

                                newTags[rusTag] += count;
                            }
                        }
                    }

                    if (newTags.Any())
                    {
                        film.RussianTags = string.Join("|", newTags.Select(p => $"{p.Key}({p.Value})")).Trim('|');
                        db.Update(film);
                    }
                } // цикл по фильмам

            }
        }


        private Dictionary<string, List<string>> GetTranslatesFromExcel(string fileName)
        {
            Excel.Application app = null;
            Dictionary<string, List<string>> translates = new Dictionary<string, List<string>>();
            try
            {
                app = new Excel.Application();
                app.DisplayAlerts = false;
                Excel.Workbook book = app.Workbooks.Open(fileName);
                Excel.Worksheet sheet = (Excel.Worksheet)book.Worksheets[1];

                int row = 1;
                int col = 1;
                string engTag = "";
                do
                {
                    engTag = sheet.Cells[row, col].Value?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(engTag))
                    {
                        translates.Add(engTag, new List<string>());
                        int j = 1;
                        string rusTag = "";
                        do
                        {
                            rusTag = sheet.Cells[row, col + j].Value?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(rusTag))
                            {
                                translates[engTag].Add(rusTag);
                            }
                            j++;
                        } while (!string.IsNullOrEmpty(rusTag));
                    }
                    row++;
                } while (!string.IsNullOrEmpty(engTag));

            }
            finally
            {
                if (app != null)
                {
                    app.Quit();
                    app.Dispose();
                }
            }

            return translates;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> translate = new Dictionary<string, string>()
            {
                { "(no genres listed)", "" },
                { "Action"          , "экшн" },
                { "Adventure"       , "приключения" },
                { "Animation"       , "мультфильм"  },
                { "Children"        , "детский"     },
                { "Comedy"          , "комедия"     },
                { "Crime"           , "криминал"    },
                { "Documentary"     , "история"     },
                { "Drama"           , "драма"       },
                { "Fantasy"         , "фэнтези"     },
                { "Film-Noir"       , "нуар"        },
                { "Horror"          , "ужасы"       },
                { "IMAX"            , ""            },
                { "Musical"         , "музыка"      },
                { "Mystery"         , "мистика"     },
                { "Romance"         , "романтика"   },
                { "Sci-Fi"          , "фантастика"  },
                { "Thriller"        , "триллер"     },
                { "War"             , "военный"     },
                { "Western"         , "вестерн" }
            };

            List<IMDbLoading> allRows = db.GetFromDatabase<IMDbLoading>()
                .Where(x => !string.IsNullOrEmpty(x.RussianTitle))
                .Where(x => !string.IsNullOrEmpty(x.EnglishGenries))
                .Where(x => string.IsNullOrEmpty(x.RussianGenries))
                .ToList();

            foreach (var film in allRows)
            {
                string[] genries = film.EnglishGenries.Split('|');
                List<string> newGenries = new List<string>();

                foreach (string genre in genries)
                {
                    if (translate.ContainsKey(genre))
                    {
                        if (!string.IsNullOrEmpty(translate[genre]))
                            newGenries.Add(translate[genre]);
                    }
                    else
                    {
                    }
                }

                if (newGenries.Any())
                {
                    film.RussianGenries = string.Join("|", newGenries);
                    db.Update(film);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            List<Film> allFilms = db.GetFromDatabase<Film>();

            List<IMDbLoading> forLoad = db.GetFromDatabase<IMDbLoading>()
                .Where(x => !string.IsNullOrEmpty(x.RussianTitle))
                .Where(x => !string.IsNullOrEmpty(x.RussianDescription))
                .Where(x => x.Rating == "N/A" || decimal.Parse(x.Rating, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) > 5.0m)
                .Where(x => !string.IsNullOrEmpty(x.RussianGenries))
                .Where(x => !allFilms.Any(y => y.Name == x.RussianTitle && y.Year == int.Parse(x.Year)))
                .ToList();

            int filmToSaveCount = 0;


            List<ActorFilm> roles = new List<ActorFilm>();
            List<FilmCustomProperty> properties = new List<FilmCustomProperty>();
            List<CountryFilm> countries = new List<CountryFilm>();
            List<ProducerFilm> producers = new List<ProducerFilm>();
            List<GenreFilm> genries = new List<GenreFilm>();

            foreach (var load in forLoad)
            {
                Film film = new Film()
                {
                    Name = load.RussianTitle,
                    Description = load.RussianDescription,
                    Rating = load.Rating == "N/A" ? (decimal?)null : decimal.Parse(load.Rating, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    Poster = load.Poster,
                    KinopoiskId = load.IMDbId,
                    Year = int.Parse(load.Year),
                    // Slogan = 
                    // Link = 
                };
                db.AddWithoutSave(film);

                string[] actorsNames = load.EnglishActors.Split(',');
                foreach (var actorName in actorsNames)
                {
                    Actor actor = FindOrCreateActor(actorName);
                    ActorFilm af = new ActorFilm()
                    {
                        Actor = actor,
                        Film = film
                    };

                    roles.Add(af);
                }

                string[] countryNames = load.EnglishCountries.Split(',');
                foreach (var countryName in countryNames)
                {
                    Country country = FindOrCreateCountry(countryName);
                    CountryFilm cf = new CountryFilm()
                    {
                        Country = country,
                        Film = film
                    };

                    countries.Add(cf);
                }

                string[] producersNames = load.EnglishProducers.Split(',');
                foreach (var producerName in producersNames)
                {
                    Producer producer = FindOrCreateProducer(producerName);
                    ProducerFilm pf = new ProducerFilm()
                    {
                        Producer = producer,
                        Film = film
                    };

                    producers.Add(pf);
                }

                List<string> tagsFromGenries = new List<string>();
                string[] genriesNames = load.RussianGenries.Split('|');
                foreach (var genreName in genriesNames)
                {
                    Genre genre = db.GetFromDatabase<Genre>(x => x.Name.ToLower() == genreName.ToLower()).FirstOrDefault();
                    GenreFilm gf = new GenreFilm()
                    {
                        Genre = genre,
                        Film = film
                    };

                    if (mapGenreToTag.ContainsKey(genreName))
                        tagsFromGenries.Add(mapGenreToTag[genreName]);
                    genries.Add(gf);
                }

                // а теперь берем теги, и сколько раз их использовали для этого фильма
                string[] tagsNames = (load.RussianTags ?? "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, decimal> tags = new Dictionary<string, decimal>();
                Regex reg = new Regex(@"(?<tag>.*)\((?<count>\d*)\)$");
                foreach (var tagStr in tagsNames)
                {
                    var match = reg.Match(tagStr);
                    string tagName = match.Groups["tag"].Value;
                    decimal count = decimal.Parse(match.Groups["count"].Value);

                    if (!tags.ContainsKey(tagName))
                        tags.Add(tagName, 0);

                    tags[tagName] += count;
                }

                // добавляем довесок от жанров
                decimal genreWeight = tags.Any()
                    ? Math.Max(tags.Max(x => x.Value) / 2m, 1)
                    : 1;
                foreach (var fromGenre in tagsFromGenries)
                {
                    if (!tags.ContainsKey(fromGenre))
                        tags.Add(fromGenre, 0);

                    tags[fromGenre] += genreWeight;
                }

                // пересчитываем на проценты
                // ну и наконец сохраняем пользоватльские свойства
                decimal maxCount = tags.Max(x => x.Value);
                foreach (var tag in tags)
                {
                    CustomProperty cp = db.GetFromDatabase<CustomProperty>().FirstOrDefault(x => x.Name.ToLower() == tag.Key.ToLower());

                    int weight = (int)(tag.Value / maxCount * 100m * 0.9m);  // * коэффициент доверия

                    if (weight > 0)
                    {
                        FilmCustomProperty fcp = new FilmCustomProperty()
                        {
                            Film = film,
                            CustomProperty = cp,
                            Value = weight
                        };

                        properties.Add(fcp);
                    }
                }

                filmToSaveCount++;
                if (filmToSaveCount >= 100)
                {
                    db.AddWithoutSave(roles);
                    db.AddWithoutSave(countries);
                    db.AddWithoutSave(producers);
                    db.AddWithoutSave(genries);
                    db.AddWithoutSave(properties);
                    db.Save();

                    roles = new List<ActorFilm>();
                    countries = new List<CountryFilm>();
                    producers = new List<ProducerFilm>();
                    genries = new List<GenreFilm>();
                    properties = new List<FilmCustomProperty>();
                    filmToSaveCount = 0;
                }
            }


            db.AddWithoutSave(roles);
            db.AddWithoutSave(countries);
            db.AddWithoutSave(producers);
            db.AddWithoutSave(genries);
            db.AddWithoutSave(properties);
            db.Save();
        }



        private Actor FindOrCreateActor(string name)
        {
            Actor actor = db.GetFromDatabase<Actor>(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (actor == null)
            {
                actor = new Actor()
                {
                    Name = name
                };
                db.AddWithoutSave(actor);
            }

            return actor;
        }


        private Country FindOrCreateCountry(string name)
        {
            Country country = db.GetFromDatabase<Country>(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (country == null)
            {
                country = new Country()
                {
                    Name = name
                };
                db.AddWithoutSave(country);
            }

            return country;
        }


        private Producer FindOrCreateProducer(string name)
        {
            Producer producer = db.GetFromDatabase<Producer>(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (producer == null)
            {
                producer = new Producer()
                {
                    Name = name
                };
                db.AddWithoutSave(producer);
            }

            return producer;
        }


        private Dictionary<string, string> mapGenreToTag = new Dictionary<string, string>()
        {
            { "экшн"        , "Экшн" },
            { "приключения" , "Приключения" },
            { "мультфильм"  , "Мультфильм" },
            { "детский"     , "Детский" },
            { "комедия"     , "Смешной" },
            { "криминал"    , "Гангстерский" },
            { "история"     , "История" },
            { "драма"       , "Драма" },
            { "фэнтези"     , "Волшебный" },
            { "нуар"        , "Америка 30-40-е" },
            { "ужасы"       , "Ужас" },
            { "музыка"      , "Музыкальный" },
            { "мистика"     , "Мистика" },
            { "романтика"   , "Романтика" },
            { "фантастика"  , "Фантастика" },
            { "триллер"     , "Боевик" },           // wololo
            { "военный"     , "Военный" },
            { "вестерн"     , "Вестерн" }
        };




    }
}
