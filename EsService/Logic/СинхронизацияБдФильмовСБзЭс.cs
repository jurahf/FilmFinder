using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class СинхронизацияБдФильмовСБзЭс
    {
        private DBWork db;
        private const string ИмяСпецДомена = "#Советы";

        public СинхронизацияБдФильмовСБзЭс()
        {
            db = new DBWork();
        }

        public void СоветыВСпецДомен()
        {
            List<Advice> advices = db.GetFromDatabase<Advice>();
            List<ExpertSystem> esList = db.GetFromDatabase<ExpertSystem>();

            foreach (var es in esList)  // TODO: вообще надо только в одну актуальную ЭС, но мы не знаем, какая это
                СоветыВСпецДомен(es, advices);
        }

        private void СоветыВСпецДомен(ExpertSystem es, List<Advice> advices)
        {
            Domain domain = es.AllDomains.Where(x => x.Name == ИмяСпецДомена).FirstOrDefault();
            if (domain == null)
            {
                domain = new Domain()
                {
                    Name = ИмяСпецДомена,
                };
            }

            es.AllDomains.Add(domain);

            foreach (var key in advices.Select(x => x.Key))
            {
                if (!domain.DomainValue.Any(x => x.Value == key.ToString()))
                {
                    domain.DomainValue.Add(new DomainValue()
                    {
                        Value = key.ToString()
                    });
                }
            }

            db.Insert(domain);
        }


    }
}
