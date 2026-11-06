namespace EsServices.Validators
{
    public class ValidationError
    {
        public ValidationError(string message, string? field = null)
        {
            Message = message;
            Field = field;
        }

        public string Message { get; }
        public string? Field { get; }
    }

    public class ValidationResult
    {
        public ValidationResult()
        {
        }

        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Errors.AddRange(errors);
        }

        public List<ValidationError> Errors { get; } = new();
        public bool Success => Errors.Count == 0;

        public void Add(string message, string? field = null) => Errors.Add(new ValidationError(message, field));
    }
}
