using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicClasses;
using ExpertSystemDb;

namespace EsStorageAdapter
{
    public interface IExpertSystemStorageAdapter
    {
        ClassicClasses.ExpertSystem CreateNewES(string name);
        ClassicClasses.ExpertSystem LoadES(string name);
        bool SaveES(ClassicClasses.ExpertSystem es, string name);
        List<string> GetAvaliableESNames();        
    }

    public class ExpertSystemStorageAdapter : IExpertSystemStorageAdapter
    {
        private DBWork db = null;
        private IDbToClassicConverter dbToClassic = null;
        private IClassicToDbConvert classicToDb = null;

        public ExpertSystemStorageAdapter()
        {
            // TODO: DI
            db = new DBWork();
            dbToClassic = new DbToClassicConverter();
            classicToDb = new ClassicToDbConvert();
        }

        public ClassicClasses.ExpertSystem CreateNewES(string name)
        {
            ExpertSystemDb.ExpertSystem es = new ExpertSystemDb.ExpertSystem()
            {
                Name = name
            };

            try
            {
                db.Insert(es); // TODO: проверки на то, что уже существует?
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при сохранении новой ЭС", ex);
            }

            return dbToClassic.ESConvert(es, new List<Consultation>());
        }


        public ClassicClasses.ExpertSystem LoadES(string name)
        {
            ExpertSystemDb.ExpertSystem fromDb = db.GetFromDatabase<ExpertSystemDb.ExpertSystem>(x => x.Name == name).FirstOrDefault();
            List<Consultation> consultationList = db.GetFromDatabase<ExpertSystemDb.Consultation>()
                .Where(x => x.ExpertSystem.Name == name)
                .ToList();

            if (fromDb == null)
                throw new KeyNotFoundException($"Экспертная система с именем {name} не найдена");

            return dbToClassic.ESConvert(fromDb, consultationList);
        }


        public List<string> GetAvaliableESNames()
        {
            return db.GetFromDatabase<ExpertSystemDb.ExpertSystem>()
                .Select(x => x.Name)
                .ToList();
        }



        public bool SaveES(ClassicClasses.ExpertSystem es, string name)
        {
            // переименовываем старую ЭС в БД
            ExpertSystemDb.ExpertSystem existed = db.GetFromDatabase<ExpertSystemDb.ExpertSystem>(x => x.Name == name).FirstOrDefault();
            if (existed != null)
            {
                existed.Name = $"{name}_auto_{DateTime.Now:yyyy-MM-dd-hh-mm-ss}";
                db.Update(existed);
            }

            try
            {
                // преобразуем классическую в новую и сохраняем
                ExpertSystemDb.ExpertSystem newEs = classicToDb.ESConvert(es, name);
                db.Insert(newEs);

                // если все ок - переименованную все равно НЕ удаляем, так как на ней висят консультации
            }
            catch (Exception ex)
            {
                // если что-то не так - переименовываем обратно
                if (existed != null)
                {
                    existed.Name = name;
                    db.Update(existed); // TODO: тут еще exception может быть
                }

                return false;
            }

            return true;
        }



    }



}
