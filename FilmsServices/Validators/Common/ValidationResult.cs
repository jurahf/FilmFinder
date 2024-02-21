using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Validators.Common
{
    public class ValidationResult
    {
        public ValidationResult(bool success)
        {
            this.Success = success;
        }

        public bool Success { get; set; } = true;

        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        public void AddResult(ValidationResult x)
        {
            Success = Success && x.Success;
            Errors.AddRange(x.Errors);
        }
    }

    public class ValidationError
    {
        public string Message { get; set; }

        public string Field { get; set; }

        public object Entity { get; set; }
    }
}
