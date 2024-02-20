using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Session : BaseEntity
    {
        public string SessionId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }
}
