﻿using System;
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
            this.AdviceCustomProperty = new HashSet<AdviceCustomProperty>();
            this.AdviceFilm = new HashSet<AdviceFilm>();

            this.Key = Guid.NewGuid();
        }

        public string FilmsStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var film in AdviceFilm)
                {
                    sb.Append($"{film.Film.Name}; ");
                }

                return sb.ToString().Trim();
            }
        }

        public string CustomPropertiesStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var cp in AdviceCustomProperty)
                {
                    sb.Append($"{cp.CustomProperty.Name}; ");
                }

                return sb.ToString().Trim();
            }
        }

        public string TagStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var pr in AdviceCustomProperty.Where(x => x.Value > 0))
                {
                    sb.Append($"{pr.CustomProperty.Name}; ");
                }

                return sb.ToString().Trim();
            }
        }
    }


}
