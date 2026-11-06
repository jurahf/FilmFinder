using EsServices.ViewModel;

namespace EsServices.Services.Advice
{
    public interface IAdviceSyncService
    {
        Task<int> SyncAsync(ExpertSystemVM expertSystem, CancellationToken cancellationToken = default);
    }
}
