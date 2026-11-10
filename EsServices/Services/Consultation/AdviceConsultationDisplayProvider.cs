using EsConsultationServices.Contracts;
using EsServices.Converters;
using EsServices.Services.Advice;

namespace EsServices.Services.Consultation
{
    /// <summary>
    /// Подставляет человекочитаемые описания советов в граф консультации.
    /// Value домена #Советы — GUID совета, Display — его описание.
    /// </summary>
    public sealed class AdviceConsultationDisplayProvider : IConsultationDisplayProvider
    {
        private readonly IAdviceCatalogService adviceCatalogService;

        public AdviceConsultationDisplayProvider(IAdviceCatalogService adviceCatalogService)
        {
            this.adviceCatalogService = adviceCatalogService;
        }

        public async Task<IReadOnlyList<ConsultationDisplayValue>> GetDisplayValuesAsync(
            CancellationToken cancellationToken = default)
        {
            var descriptions = await adviceCatalogService.GetDescriptionsAsync(cancellationToken);

            return descriptions
                .Select(pair => new ConsultationDisplayValue
                {
                    DomainName = ExpertSystemConverter.AdviceDomainName,
                    Value = pair.Key,
                    Display = pair.Value
                })
                .ToList();
        }
    }
}
