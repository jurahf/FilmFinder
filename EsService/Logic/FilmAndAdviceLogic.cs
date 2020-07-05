using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class FilmAndAdviceLogic
    {
        IDataWork db;

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

        public List<Film> FindFilmsByAdviceGuid(Guid guid)
        {
            Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

            if (advice == null)
                throw new ArgumentNullException();

            return FindFilmsByAdvice(advice);
        }

        public List<Film> FindFilmsByAdvice(Advice advice)
        {
            // набор свойств совета и их значения - это вектор
            // ищем все связи с фильмами этих пользоватеских советов - сначала просто все фильмы
            List<Film> films = new List<Film>();
            foreach (var prop in advice.AdviceCustomProperty)
            {
                films = films.Union(
                    db.GetFromDatabase<FilmCustomProperty>(x => x.CustomProperty.Id == prop.CustomProperty.Id)
                    .Select(x => x.Film))
                    .ToList();
            }

            // оставляем только фильмы с теми же знаками в значениях свойств (вычисляем нужную "четверть" на координатной оси)
            List<Film> filteredFilms = new List<Film>(films);
            foreach (AdviceCustomProperty adviceProp in advice.AdviceCustomProperty)
            {
                foreach (var film in films)
                {
                    FilmCustomProperty filmProp = film.FilmCustomProperty
                        .FirstOrDefault(x => x.CustomProperty.Id == adviceProp.CustomProperty.Id);

                    if (filmProp.Value * adviceProp.Value < 0) // если == 0, то пофиг, пусть будет
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
            // TODO: отдавать все, а уже клиент пусть решает, топ ему надо или не топ
            return result.OrderBy(x => x.Distance)
                .Take(5)
                .Select(x => x.Film)
                .ToList();
        }



    }


    public class FilmRelevance
    {
        public Film Film { get; set; }
        public double Distance { get; set; }
    }



}
