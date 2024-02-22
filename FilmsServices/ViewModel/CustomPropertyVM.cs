using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.ViewModel
{
    public class CustomPropertyVM : BaseViewModel
    {
        public int Value { get; set; }

        public string Name { get; set; }


        public override string ToString()
        {
            return $"{Name} ({Value})";
        }
    }
}
