using EsServices.Validators;
using EsServices.ViewModel;

namespace EsServices.Services
{
    public interface IExpertSystemService
    {
        Task<List<ExpertSystemSummaryVM>> GetListAsync(CancellationToken cancellationToken = default);
        Task<ExpertSystemVM> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<ExpertSystemVM> CreateAsync(string name, CancellationToken cancellationToken = default);
        Task<SaveExpertSystemResult> SaveAsNewVersionAsync(ExpertSystemVM expertSystem, CancellationToken cancellationToken = default);
    }

    public class SaveExpertSystemResult
    {
        public ValidationResult Validation { get; init; } = new();

        public ExpertSystemVM? ExpertSystem { get; init; }

        public string? Error { get; init; }

        public bool Success => Validation.Success && Error == null && ExpertSystem != null;
    }
}
