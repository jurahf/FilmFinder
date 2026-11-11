using FilmDb.Repositories;

namespace EsServices.Services.Advice
{
    /// <summary>
    /// Описания советов для UI. Построение текста живёт в FilmDb,
    /// чтобы UI и консультация показывали одинаковое описание.
    /// </summary>
    public class AdviceCatalogService : IAdviceCatalogService
    {
        private readonly IAdviceRepository adviceRepository;

        public AdviceCatalogService(IAdviceRepository adviceRepository)
        {
            this.adviceRepository = adviceRepository;
        }

        public Task<IReadOnlyDictionary<string, string>> GetDescriptionsAsync(CancellationToken cancellationToken = default)
        {
            return adviceRepository.GetDescriptionsAsync(cancellationToken);
        }

        public Task<IReadOnlyDictionary<string, string>> GetDescriptionsByKeyAsync(CancellationToken cancellationToken = default)
        {
            return adviceRepository.GetDescriptionsAsync(cancellationToken);
        }
    }
}
