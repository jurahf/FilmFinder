using EsConsultationServices.Engine;

namespace EsConsultationServices.Persistence
{
    internal sealed class ConsultationSessionState
    {
        public ConsultationSessionState(
            string sessionId,
            int consultationId,
            int expertSystemId,
            string expertSystemName,
            ConsultationEngineState engineState)
        {
            SessionId = sessionId;
            ConsultationId = consultationId;
            ExpertSystemId = expertSystemId;
            ExpertSystemName = expertSystemName;
            EngineState = engineState;
        }

        public string SessionId { get; }
        public int ConsultationId { get; }
        public int ExpertSystemId { get; }
        public string ExpertSystemName { get; }
        public ConsultationEngineState EngineState { get; }
    }

    internal sealed record PendingAnswer(int VariableId, int? DomainValueId, string Value);
}
