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
            //advice.AdviceCustomPropertyPositive.Select(x => x.
            throw new NotImplementedException();

            // составляем вектор по пользовательским свойствам этого совета
            // ищем все связи с фильмами этих пользоватеских советов
            // по ним тоже составляем вектора
            // из каждого вектора фильма вычитаем вектор совета
            // элементы полученных разностей берем по модулю и складываем
            // сортируем в порядке возрастания
            // берем топ фильмов

            // привязанные положительные фильмы добавляем в выходной поток
            // привязанные негативные фильмы убираем из полученного списка
        }

    }
}
