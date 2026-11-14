using EsServices.Services;
using EsServices.Services.Export;
using EsServices.ViewModel;
using Xunit;

namespace EsServices.Tests
{
    public class ExpertSystemExportServiceTests
    {
        [Fact]
        public async Task Export_SelectsExactFilmsAndIgnoresArchived()
        {
            var service = CreateService(
                new ExpertSystemSummaryVM { Id = 1, Name = "Фильмы_auto_2026-09-09" },
                new ExpertSystemSummaryVM { Id = 2, Name = "Фильмы" },
                new ExpertSystemSummaryVM { Id = 3, Name = " фильмы " });

            var result = await service.ExportMermaidAsync();

            Assert.Equal(3, result.ExpertSystemId);
        }

        [Fact]
        public async Task Export_BuildsDefaultFileName()
        {
            var service = CreateService(new ExpertSystemSummaryVM { Id = 1, Name = "Фильмы" });

            var result = await service.ExportMermaidAsync();

            Assert.Matches(@"^expert-system-\d{4}-\d{2}-\d{2}\.mmd$", result.FileName);
        }

        [Fact]
        public async Task Export_ThrowsWhenFilmsNotFound()
        {
            var service = CreateService(
                new ExpertSystemSummaryVM { Id = 1, Name = "Фильмы_auto_2026-09-09" });

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ExportMermaidAsync());
        }

        [Fact]
        public async Task Export_ReturnsContentWithTargetVector()
        {
            var service = CreateService(new ExpertSystemSummaryVM { Id = 1, Name = "Фильмы" });

            var result = await service.ExportMermaidAsync();

            Assert.Equal("Фильмы", result.ExpertSystemName);
            Assert.Equal(1, result.RuleCount);
            Assert.Contains("P1{{\"T = Да\"}}:::property", result.Content);
        }

        private static ExpertSystemExportService CreateService(
            params ExpertSystemSummaryVM[] items)
        {
            var fakeService = new FakeExpertSystemService(items);

            return new ExpertSystemExportService(
                fakeService,
                new ExpertSystemMermaidExporter());
        }

        private sealed class FakeExpertSystemService : IExpertSystemService
        {
            private readonly List<ExpertSystemSummaryVM> items;

            public FakeExpertSystemService(IEnumerable<ExpertSystemSummaryVM> items)
            {
                this.items = items.ToList();
            }

            public Task<List<ExpertSystemSummaryVM>> GetListAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(items.ToList());
            }

            public Task<ExpertSystemVM> GetAsync(
                int id,
                CancellationToken cancellationToken = default)
            {
                var expertSystem = TestExpertSystemFactory.BuildSimple();
                expertSystem.Id = id;
                expertSystem.Name = "Фильмы";

                return Task.FromResult(expertSystem);
            }

            public Task<ExpertSystemVM> CreateAsync(
                string name,
                CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            public Task<SaveExpertSystemResult> SaveAsNewVersionAsync(
                ExpertSystemVM expertSystem,
                CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }
        }
    }
}
