using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Rule : BaseEntity
    {
        public Rule()
        {
            this.Conditions = new HashSet<RuleFact>();
        }

        public int Order { get; set; }
        public string Name { get; set; }
        public string Reasoning { get; set; }

        public virtual Fact Result { get; set; }

        public virtual ICollection<RuleFact> Conditions { get; set; }
    }
}
