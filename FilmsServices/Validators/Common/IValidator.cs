using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Validators.Common
{
    public interface IValidator<VM>
        where VM : BaseViewModel
    {
        ValidationResult Validate(VM vm);
    }
}
