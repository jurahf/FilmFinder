using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class CountryFilm : BaseEntity
    {
        public virtual Film Film { get; set; }
        public virtual Country Country { get; set; }
    }
}
