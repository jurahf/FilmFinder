using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertSystemDb
{
    public partial class Advice
    {
        // TODO: вообще-то он будет перегенерироваться все время
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Advice()
        {
            this.AdviceCustomPropertyPositive = new HashSet<AdviceCustomProperty>();
            this.AdviceFilmPositive = new HashSet<AdviceFilm>();
            this.AdviceCustomPropertyNegative = new HashSet<AdviceCustomProperty>();
            this.AdviceFilmNegative = new HashSet<AdviceFilm>();

            this.Key = Guid.NewGuid();
        }

        public string FilmsStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var film in AdviceFilmPositive)
                {
                    sb.Append($"{film.Film.Name}; ");
                }

                return sb.ToString().Trim();
            }
        }

        public string TagStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var pr in AdviceCustomPropertyPositive)
                {
                    sb.Append($"{pr.CustomProperty.Name}: {pr.CustomProperty.Value}; ");
                }

                return sb.ToString().Trim();
            }
        }
    }


}
