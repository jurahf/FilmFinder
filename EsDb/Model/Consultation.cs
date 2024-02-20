using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Consultation : BaseEntity
    {
        public Consultation()
        {
            this.ProvedFacts = new HashSet<ConsultationFact>();
            this.WorkedRules = new HashSet<ConsultationRule>();
        }


        public virtual Session Session { get; set; }
        public virtual ExpertSystem ExpertSystem { get; set; }
        public virtual ICollection<ConsultationFact> ProvedFacts { get; set; }
        public virtual ICollection<ConsultationRule> WorkedRules { get; set; }
        public virtual Rule CurrentRule { get; set; }
        public virtual GoalStack GoalStack { get; set; }
        public virtual FinalSolution FinalSolution { get; set; }
    }
}
