using EsConsultationServices.Contracts;
using FilmDb.Repositories;

namespace EsConsultationServices.Films;

/// <summary>
/// Подставляет человекочитаемые описания советов в граф консультации.
/// Value домена #Советы — GUID совета, Display — его описание.
/// </summary>
public sealed class AdviceConsultationDisplayProvider : IConsultationDisplayProvider
{
    private readonly IAdviceRepository adviceRepository;

    public AdviceConsultationDisplayProvider(IAdviceRepository adviceRepository)
    {
        this.adviceRepository = adviceRepository;
    }

    public async Task<IReadOnlyList<ConsultationDisplayValue>> GetDisplayValuesAsync(
        CancellationToken cancellationToken = default)
    {
        var descriptions = await adviceRepository.GetDescriptionsAsync(cancellationToken);

        return descriptions
            .Select(pair => new ConsultationDisplayValue
            {
                DomainName = AdviceCatalogConstants.DomainName,
                Value = pair.Key,
                Display = pair.Value
            })
            .ToList();
    }
}
