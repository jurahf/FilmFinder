using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class FilmAndAdviceLogic
    {
        public const string GenreAndTagResultVariableName = "ФильтрПоЖанрамИТэгамДляСессии";
        private readonly IDataWork db;

        public FilmAndAdviceLogic()
        {
            db = new DBWork();
        }

        public string GetAdviceDescription(Guid guid)
        {
            Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

            if (advice == null)
                return "Не найден";

            return $"{advice.FilmsStr}{advice.CustomPropertiesStr}";
        }

        private List<Film> FindFilmsByAdviceGuid(Guid guid)
        {
            Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

            if (advice == null)
                throw new ArgumentNullException();

            return FindFilmsByAdvice(advice);
        }

        private List<Film> FindFilmsByAdvice(Advice advice)
        {
            // набор свойств совета и их значения - это вектор
            // ищем все связи с фильмами этих пользоватеских советов - сначала просто все фильмы
            List<Film> films = new List<Film>();
            foreach (var prop in advice.AdviceCustomProperty)
            {
                if (prop.Value >= 0)
                {
                    films = films.Union(
                        db.GetFromDatabase<FilmCustomProperty>(x => x.CustomProperty.Id == prop.CustomProperty.Id)
                        .Select(x => x.Film))
                        .ToList();
                }
            }

            // оставляем только фильмы с теми же знаками в значениях свойств (вычисляем нужную "четверть" на координатной оси)
            List<Film> filteredFilms = new List<Film>(films);
            foreach (AdviceCustomProperty adviceProp in advice.AdviceCustomProperty)
            {
                foreach (var film in films)
                {
                    FilmCustomProperty filmProp = film.FilmCustomProperty
                        .FirstOrDefault(x => x.CustomProperty.Id == adviceProp.CustomProperty.Id);

                    if (filmProp != null && filmProp.Value * adviceProp.Value < 0) // если == 0, то пофиг, пусть будет
                        filteredFilms = filteredFilms.Where(x => x.Id != film.Id).ToList();
                }
            }

            // значения свойств в фильмах - тоже вектора
            // считаем расстояния между вектором совета и каждым вектором фильма
            // но здесь уже надо учитывать все координаты, а не только те, которые были в коллекции
            List<CustomProperty> allProps = db.GetFromDatabase<CustomProperty>();
            List<FilmRelevance> result = new List<FilmRelevance>();
            foreach (var film in filteredFilms)
            {
                double distance = 0;
                foreach (var prop in allProps)
                {
                    double adviceCoord = advice.AdviceCustomProperty
                        .FirstOrDefault(x => x.CustomProperty.Id == prop.Id)
                        ?.Value ?? 0;
                    double filmCoord = film.FilmCustomProperty
                        .FirstOrDefault(x => x.CustomProperty.Id == prop.Id)
                        ?.Value ?? 0;

                    distance += Math.Pow(adviceCoord - filmCoord, 2);
                }

                result.Add(new FilmRelevance() { Film = film, Distance = distance });
            }

            // берем топ
            int percent25 = Math.Max((int)(result.Count * 0.25), 5);
            return result.OrderBy(x => x.Distance)
                .ThenByDescending(x => x.Film.Rating ?? 0)
                .Take(percent25)
                .TakeRandom(5)
                .Select(x => x.Film)
                .ToList();
        }

        
        private List<Film> FindFilmsByFilters(PreprocessQuestions filters)
        {
            // filters.GenreForFilter
            List<Film> filmsList = db.GetFromDatabase<Film>();

            foreach (var g in filters.GenreForFilter)
            {
                filmsList = filmsList.Where(x => x.GenreFilm.Select(y => y.Genre.Id).Contains(g.Genre.Id))
                    .ToList();
            }

            int percent25 = Math.Max((int)(filmsList.Count * 0.25), 5);
            return filmsList.OrderByDescending(x => x.Rating ?? 0)
                .Take(percent25)
                .TakeRandom(5)
                .ToList();

            // TODO: filters.CustomPropertyForFilter - тут так же, как с советом, но пока не используется
        }


        public FilmDto FindFilmByConsultResult(ConsultResultDto result)
        {
            List<Film> filmList = new List<Film>();

            if (result.Fact.VarName == GenreAndTagResultVariableName)
            {
                // результат из фильтров жанров и тэгов
                Session session = db.GetFromDatabase<Session>(x => x.SessionId == result.Fact.Value).FirstOrDefault();

                if (session?.PreprocessQuestions == null)
                    throw new ArgumentException("Не найдена сессия пользователя");

                filmList = FindFilmsByFilters(session.PreprocessQuestions);
            }
            else
            {
                // результат из ЭС
                string key = result.Fact.Value;
                Guid guid;
                if (!Guid.TryParse(key, out guid))
                    throw new Exception("Не найден guid результата");

                Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

                if (advice == null)
                    throw new Exception("Не найден результат");

                filmList = FindFilmsByAdvice(advice);
            }

            if (filmList.Any())
            {
                var filmDtos = filmList.Select(x => new FilmDto(x)).ToList();

                int index = new Random().Next(0, filmDtos.Count - 1);
                FilmDto film = filmDtos[index];

                return film;
            }
            else
            {
                return null;
            }
        }


    }


    public static class Extensions
    {
        public static IEnumerable<T> TakeRandom<T>(this IOrderedEnumerable<T> enumerable, int count)
        {
            if (enumerable.Count() <= count)
            {
                foreach (var res in enumerable)
                    yield return res;
            }

            List<T> result = new List<T>();
            List<int> selectedIndexes = new List<int>();

            Random random = new Random();

            while (selectedIndexes.Count < count)
            {
                int index = -1;
                do
                {
                    index = random.Next(0, enumerable.Count() - 1);
                } while (selectedIndexes.Contains(index));

                selectedIndexes.Add(index);
            }

            foreach (var index in selectedIndexes)
                yield return enumerable.ElementAt(index);
        }

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> enumerable, int count)
        {
            if (enumerable.Count() <= count)
            {
                foreach (var res in enumerable)
                    yield return res;
                yield break;
            }

            List<T> result = new List<T>();
            List<int> selectedIndexes = new List<int>();

            Random random = new Random();

            while (selectedIndexes.Count < count)
            {
                int index = -1;
                do
                {
                    index = random.Next(0, enumerable.Count());
                } while (selectedIndexes.Contains(index));

                selectedIndexes.Add(index);
            }

            foreach (var index in selectedIndexes)
                yield return enumerable.ElementAt(index);
        }
    }



    public class FilmRelevance
    {
        public Film Film { get; set; }
        public double Distance { get; set; }
    }



}
