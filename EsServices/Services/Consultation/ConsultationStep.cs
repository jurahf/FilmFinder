using EsServices.ViewModel.Consultation;

namespace EsServices.Services.Consultation
{
    public class ConsultationStep
    {
        public ConsultationQuestionVM? Question { get; init; }
        public ConsultationResultVM? Result { get; init; }
        public bool IsCompleted => Result != null;
    }
}
