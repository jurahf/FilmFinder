using CommonRepositories;
using EsDb.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class ConsultationFact : BaseEntity
    {
        public FactTruly Truly { get; set; }

        public virtual Consultation Consultation { get; set; }
        public virtual Fact Fact { get; set; }
    }
}
