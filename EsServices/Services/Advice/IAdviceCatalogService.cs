namespace EsServices.Services.Advice
{
    public interface IAdviceCatalogService
    {
        Task<IReadOnlyDictionary<string, string>> GetDescriptionsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<string, string>> GetDescriptionsByKeyAsync(CancellationToken cancellationToken = default);
    }
}
