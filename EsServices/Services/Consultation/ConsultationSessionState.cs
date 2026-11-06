using EsServices.ViewModel;

namespace EsServices.Services.Consultation
{
    public class ConsultationSessionState
    {
        public ConsultationSessionState(
            string sessionId,
            ExpertSystemVM expertSystem,
            ConsultationEngineState engineState)
        {
            SessionId = sessionId;
            ExpertSystem = expertSystem;
            EngineState = engineState;
        }

        public string SessionId { get; }
        public ExpertSystemVM ExpertSystem { get; }
        public ConsultationEngineState EngineState { get; }
        public int? ConsultationId { get; set; }
        public string? PersistenceError { get; set; }
    }
}
