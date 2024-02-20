using CommonRepositories;
using EsDb.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class ConsultationRule : BaseEntity
    {
        public RuleWork Work { get; set; }

        public virtual Consultation Consultation { get; set; }
        public virtual Rule Rule { get; set; }
    }
}
