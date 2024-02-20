using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class ExpertSystem : BaseEntity
    {
        public ExpertSystem()
        {
            this.AllDomains = new HashSet<Domain>();
            this.AllVariables = new HashSet<Variable>();
            this.AllRules = new HashSet<Rule>();
        }

        public string Name { get; set; }

        public virtual ICollection<Domain> AllDomains { get; set; }
        public virtual ICollection<Variable> AllVariables { get; set; }
        public virtual Variable Target { get; set; }
        public virtual ICollection<Rule> AllRules { get; set; }
    }
}
