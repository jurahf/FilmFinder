namespace EsServices.ViewModel.Consultation
{
    public class ConsultationQuestionVM
    {
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<ConsultationAnswerVM> Answers { get; set; } = new();
    }
}
