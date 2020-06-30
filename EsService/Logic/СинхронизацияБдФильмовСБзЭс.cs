using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    /// <summary>
    /// Для отправки GUID советов в специальный домен в ЭС, чтобы потом можно было указать его в качестве результата консультации
    /// </summary>
    public class СинхронизацияБдФильмовСБзЭс
    {
        private IDataWork db;
        private const string ИмяСпецДомена = "#Советы";

        public СинхронизацияБдФильмовСБзЭс()
        {
            db = new DBWork();
        }

        /// <summary>
        /// Возвращает значение домена или красивое (синхронизированное) значение, если это спец. домен
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public ОтображениеЗначенияДомена ЗначениеДомена(ClassicClasses.ValueDomain domain, int pos)
        {
            if (domain.Name == ИмяСпецДомена)
            {
                string guidStr = domain.GetVal(pos);

                return ЗначениеДомена(domain, guidStr);
            }

            string usualResult = domain.GetVal(pos);
            return new ОтображениеЗначенияДомена(usualResult, usualResult);
        }

        /// <summary>
        /// Возвращает значение домена или красивое (синхронизированное) значение, если это спец. домен
        /// </summary>
        public ОтображениеЗначенияДомена ЗначениеДомена(ClassicClasses.ValueDomain domain, string value)
        {
            if (domain.Name == ИмяСпецДомена && Guid.TryParse(value, out Guid guid))
            {
                FilmAndAdviceLogic fl = new FilmAndAdviceLogic();
                string desc = fl.GetAdviceDescription(guid);

                return new ОтображениеЗначенияДомена(value, desc);
            }
            else
            {
                return new ОтображениеЗначенияДомена(value, value);
            }
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




    public class ОтображениеЗначенияДомена
    {
        public string ВнутреннееПредствление { get; set; }
        public string КрасивоеПредставление { get; set; }

        public ОтображениеЗначенияДомена(string внутреннее, string красивое)
        {
            ВнутреннееПредствление = внутреннее;
            КрасивоеПредставление = красивое;
        }

        public override string ToString()
        {
            return КрасивоеПредставление;
        }
    }


    // TODO: смысл не такой как у ОтображениеЗначенияДомена
    public class ОтображениеФакта
    {
        private СинхронизацияБдФильмовСБзЭс синхр = new СинхронизацияБдФильмовСБзЭс();

        public ClassicClasses.Fact Факт { get; set; }
        public string Отображение
        {
            get
            {
                return Факт.V.Name + " = " + синхр.ЗначениеДомена(Факт.V.Domain, Факт.Weight);
            }
        }

        public ОтображениеФакта(ClassicClasses.Fact Факт)
        {
            this.Факт = Факт;
        }

        public override string ToString()
        {
            return Отображение;
        }
    }

    public class ОтображениеПравила
    {
        private СинхронизацияБдФильмовСБзЭс синхр = new СинхронизацияБдФильмовСБзЭс();

        public ClassicClasses.Rule Правило { get; set; }
        public string Отображение
        {
            get
            {
                StringBuilder sb = new StringBuilder($"{Правило.Name}: ");

                if (Правило.Reasons.Count > 0)
                {
                    sb.Append("ЕСЛИ ");
                    for (int i = 0; i < Правило.Reasons.Count; i++)
                    {
                        sb.Append($"({new ОтображениеФакта(Правило.Reasons[i]).Отображение})");

                        if (i < Правило.Reasons.Count - 1)
                            sb.Append(" И ");
                    }

                    sb.Append($" ТОГДА ");

                    if (Правило.Result != null)
                        sb.Append(new ОтображениеФакта(Правило.Result).Отображение);
                }

                return sb.ToString();
            }
        }

        public ОтображениеПравила(ClassicClasses.Rule Правило)
        {
            this.Правило = Правило;
        }

        public override string ToString()
        {
            return Отображение;
        }
    }



}
