using EsConsultationServices.Contracts;

namespace EsConsultationServices.Services
{
    /// <summary>
    /// Провайдер по умолчанию: показывает исходные значения домена.
    /// Приложение может зарегистрировать свой провайдер (например, описания советов).
    /// </summary>
    public sealed class DefaultConsultationDisplayProvider : IConsultationDisplayProvider
    {
        public Task<IReadOnlyList<ConsultationDisplayValue>> GetDisplayValuesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ConsultationDisplayValue>>(
                Array.Empty<ConsultationDisplayValue>());
        }
    }
}
