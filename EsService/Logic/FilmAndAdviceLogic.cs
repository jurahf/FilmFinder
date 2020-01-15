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

    }
}
