using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Validators.Common
{
    public class DefaultValidator<VM> : IValidator<VM>
        where VM : BaseViewModel
    {
        public ValidationResult Validate(VM vm)
        {
            return new ValidationResult(true);
        }
    }
}
