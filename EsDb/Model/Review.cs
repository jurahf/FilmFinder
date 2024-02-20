using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Review : BaseEntity
    {
        public int Estimate { get; set; }
        public string Comment { get; set; }

        public virtual Session Session { get; set; }
    }
}
