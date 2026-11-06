using EsServices.Converters;
using EsServices.ViewModel;

namespace EsServices.Services.Advice
{
    public class AdviceSyncService : IAdviceSyncService
    {
        private readonly IAdviceCatalogService adviceCatalog;

        public AdviceSyncService(IAdviceCatalogService adviceCatalog)
        {
            this.adviceCatalog = adviceCatalog;
        }

        public async Task<int> SyncAsync(ExpertSystemVM expertSystem, CancellationToken cancellationToken = default)
        {
            var descriptions = await adviceCatalog.GetDescriptionsAsync(cancellationToken);

            var domain = expertSystem.Domains
                .FirstOrDefault(x => string.Equals(x.Name, ExpertSystemConverter.AdviceDomainName, StringComparison.Ordinal));

            if (domain == null)
            {
                domain = new DomainVM { Name = ExpertSystemConverter.AdviceDomainName };
                expertSystem.Domains.Add(domain);
            }

            int added = 0;
            foreach (var pair in descriptions.OrderBy(x => x.Value))
            {
                if (domain.Values.Any(x => string.Equals(x.Value, pair.Key, StringComparison.OrdinalIgnoreCase)))
                    continue;

                domain.Values.Add(new DomainValueVM
                {
                    Value = pair.Key,
                    Display = pair.Value
                });
                added++;
            }

            return added;
        }
    }
}
