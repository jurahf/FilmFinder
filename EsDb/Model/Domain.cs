using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Domain : BaseEntity
    {
        public Domain()
        {
            this.DomainValue = new HashSet<DomainValue>();
        }

        public string Name { get; set; }

        public virtual ICollection<DomainValue> DomainValue { get; set; }
    }
}
