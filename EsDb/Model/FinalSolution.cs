using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class FinalSolution : BaseEntity
    {
        public string VariableName { get; set; }
        public string Value { get; set; }

        public virtual Consultation Consultation { get; set; }
    }
}
