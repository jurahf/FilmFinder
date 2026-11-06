using EsDb.Model.Enums;

namespace EsServices.ViewModel.Consultation
{
    public class ConsultationExplanationVM
    {
        public List<ConsultationExplanationRuleVM> Rules { get; set; } = new();
        public List<string> ProvedFacts { get; set; } = new();
        public List<string> UnknownVariables { get; set; } = new();
    }

    public class ConsultationExplanationRuleVM
    {
        public string Name { get; set; } = string.Empty;
        public string? Reasoning { get; set; }
        public RuleWork Work { get; set; }
        public List<string> Conditions { get; set; } = new();
        public string Result { get; set; } = string.Empty;

        public string WorkDisplay => Work switch
        {
            RuleWork.Signify => "сработало",
            RuleWork.UnSignify => "не сработало",
            _ => "не проверялось"
        };
    }
}
