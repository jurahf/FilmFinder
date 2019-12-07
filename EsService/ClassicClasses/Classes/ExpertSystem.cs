using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace ClassicClasses
{
    [Serializable]
    public class ExpertSystem
    {        
        /// <summary>
        /// Список всех доменов значений ЭС (имя, домен)
        /// </summary>
        private OrderedDictionary<string, ValueDomain> domains = new OrderedDictionary<string, ValueDomain>();
        /// <summary>
        /// Список всех переменных ЭС (имя, переманная)
        /// </summary>
        private OrderedDictionary<string, Variable> vars = new OrderedDictionary<string, Variable>();
        /// <summary>
        /// Список правил
        /// </summary>
        private OrderedDictionary<string, Rule> rules = new OrderedDictionary<string, Rule>();
        /// <summary>
        /// Цель консультации
        /// </summary>
        private Variable goal;



        /// <summary>
        /// Доказанные факты
        /// </summary>
        private List<Fact> proved = new List<Fact>();
        /// <summary>
        /// Сработавшие правила
        /// </summary>
        private List<Rule> workedRules;
        public string res;


        #region Свойства
        public List<Rule> WorkedRules
        {
            get { return workedRules; }
        }

        public List<Fact> Proved
        {
            get { return proved; }
        }

        public Variable Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        public OrderedDictionary<string, ValueDomain> Domains
        {
            get { return domains; }
            set { domains = value; }
        }

        public OrderedDictionary<string, Variable> Vars
        {
            get { return vars; }
            set { vars = value; }
        }

        public OrderedDictionary<string, Rule> Rules
        {
            get { return rules; }
            set { rules = value; }
        }
        #endregion

        #region Конструктор
        public ExpertSystem()
        { 
        }
        #endregion


    }

}
