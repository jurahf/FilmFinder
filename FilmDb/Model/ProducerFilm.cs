using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class ProducerFilm : BaseEntity
    {
        public virtual Producer Producer { get; set; }
        public virtual Film Film { get; set; }
    }
}
