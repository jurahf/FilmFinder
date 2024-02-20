using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FilmDb.Model
{
    public class FilmCustomProperty : BaseEntity
    {
        public int Value { get; set; }

        public virtual Film Film { get; set; }
        public virtual CustomProperty CustomProperty { get; set; }
    }
}
