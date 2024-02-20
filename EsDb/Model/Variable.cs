using CommonRepositories;
using EsDb.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Variable : BaseEntity
    {
        public string Name { get; set; }
        public string Question { get; set; }
        public string Reasoning { get; set; }
        public VariableType Type { get; set; }

        public virtual Domain Domain { get; set; }
    }
}
