namespace EsServices.Services.Consultation
{
    /// <summary>
    /// Состояние консультаций в рамках Blazor-цепочки (scoped).
    /// </summary>
    public class ConsultationSessionStore
    {
        private readonly Dictionary<string, ConsultationSessionState> sessions = new(StringComparer.Ordinal);

        public void Add(ConsultationSessionState state) => sessions[state.SessionId] = state;

        public ConsultationSessionState Get(string sessionId)
        {
            if (!sessions.TryGetValue(sessionId, out var state))
                throw new InvalidOperationException("Консультация не найдена. Начните консультацию заново.");

            return state;
        }

        public bool TryGet(string sessionId, out ConsultationSessionState state)
            => sessions.TryGetValue(sessionId, out state!);

        public void Remove(string sessionId) => sessions.Remove(sessionId);
    }
}
