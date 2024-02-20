using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class GenreFilm : BaseEntity
    {
        public virtual Film Film { get; set; }
        public virtual Genre Genre { get; set; }
    }
}
