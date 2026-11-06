namespace EsServices.ViewModel.Consultation
{
    public class ConsultationResultVM
    {
        public string VariableName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public bool IsUnknown { get; set; }
        public ConsultationExplanationVM Explanation { get; set; } = new();
    }
}
