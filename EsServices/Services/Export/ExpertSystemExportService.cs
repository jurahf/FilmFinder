using EsServices.Services;

namespace EsServices.Services.Export
{
    /// <summary>
    /// Экспорт активной экспертной системы «Фильмы» в Mermaid.
    /// </summary>
    public sealed class ExpertSystemExportService : IExpertSystemExportService
    {
        public const string DefaultExpertSystemName = "Фильмы";

        private readonly IExpertSystemService expertSystemService;
        private readonly ExpertSystemMermaidExporter exporter;

        public ExpertSystemExportService(
            IExpertSystemService expertSystemService,
            ExpertSystemMermaidExporter exporter)
        {
            this.expertSystemService = expertSystemService;
            this.exporter = exporter;
        }

        public async Task<ExpertSystemExportResult> ExportMermaidAsync(
            CancellationToken cancellationToken = default)
        {
            var items = await expertSystemService.GetListAsync(cancellationToken);

            var summary = items
                .Where(x => string.Equals(
                    x.Name.Trim(),
                    DefaultExpertSystemName,
                    StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            if (summary == null)
                throw new KeyNotFoundException($"Экспертная система '{DefaultExpertSystemName}' не найдена.");

            var expertSystem = await expertSystemService.GetAsync(summary.Id, cancellationToken);

            var now = DateTime.Now;
            var content = exporter.Export(expertSystem, now);

            return new ExpertSystemExportResult
            {
                ExpertSystemId = expertSystem.Id,
                ExpertSystemName = expertSystem.Name,
                RuleCount = expertSystem.Rules.Count,
                FileName = $"expert-system-{now:yyyy-MM-dd}.mmd",
                Content = content
            };
        }
    }
}
