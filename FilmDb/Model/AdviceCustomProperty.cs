using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class AdviceCustomProperty : BaseEntity
    {
        public int Value { get; set; }

        public virtual int CustomPropertyId { get; set; }

        public virtual CustomProperty CustomProperty { get; set; }

        public virtual int AdviceId { get; set; }

        public virtual Advice Advice { get; set; }
    }
}
