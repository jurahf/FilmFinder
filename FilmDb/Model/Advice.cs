using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class Advice : BaseEntity
    {
        public Advice()
        {
            AdviceFilm = new HashSet<AdviceFilm>();
            AdviceCustomProperty = new HashSet<AdviceCustomProperty>();
        }

        public Guid Key { get; set; }


        public virtual ICollection<AdviceFilm> AdviceFilm { get; set; }

        public virtual ICollection<AdviceCustomProperty> AdviceCustomProperty { get; set; }
    }
}
