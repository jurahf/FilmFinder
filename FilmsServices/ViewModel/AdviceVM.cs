using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.ViewModel
{
    public class AdviceVM : BaseViewModel
    {
        public AdviceVM()
        {
            Films = new HashSet<FilmVM>();
            CustomProperties = new HashSet<CustomPropertyVM>();
        }

        public Guid Key { get; set; }


        public virtual ICollection<FilmVM> Films { get; set; }

        public virtual ICollection<CustomPropertyVM> CustomProperties { get; set; }
    }
}
