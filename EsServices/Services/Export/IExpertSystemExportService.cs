namespace EsServices.Services.Export
{
    public interface IExpertSystemExportService
    {
        Task<ExpertSystemExportResult> ExportMermaidAsync(CancellationToken cancellationToken = default);
    }

    public sealed class ExpertSystemExportResult
    {
        public int ExpertSystemId { get; init; }
        public string ExpertSystemName { get; init; } = string.Empty;
        public int RuleCount { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }
}
