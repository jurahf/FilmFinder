using EsConsultationServices;
using EsConsultationServices.Persistence;
using EsDb;
using EsDb.Model;
using EsDb.Model.Enums;
using EsDb.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EsServices.Tests
{
    public class ConsultationExpertSystemCacheTests
    {
        [Fact]
        public async Task CacheLoadsGraphOnceAndReloadsAfterInvalidate()
        {
            await using var connection = new SqliteConnection("Filename=:memory:");
            await connection.OpenAsync();

            var counter = new RepositoryCallCounter();
            var services = new ServiceCollection();
            services.AddDbContext<EsDbContext>(options => options.UseSqlite(connection));
            services.AddSingleton(counter);
            services.AddScoped<IExpertSystemRepository>(provider =>
                new CountingExpertSystemRepository(
                    new ExpertSystemRepository(provider.GetRequiredService<EsDbContext>()),
                    counter));
            services.AddEsConsultationServices();

            await using var provider = services.BuildServiceProvider();

            int expertSystemId;
            await using (var scope = provider.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EsDbContext>();
                await db.Database.EnsureCreatedAsync();
                expertSystemId = await ConsultationTestData.SeedExpertSystemAsync(
                    db,
                    "Cached",
                    VariableType.Detective);
            }

            var cache = provider.GetRequiredService<ConsultationExpertSystemCache>();

            var first = await cache.GetAsync(expertSystemId);
            var second = await cache.GetAsync(expertSystemId);

            Assert.Same(first, second);
            Assert.Equal(1, counter.GetGraphCalls);

            cache.Invalidate(expertSystemId);

            var third = await cache.GetAsync(expertSystemId);

            Assert.NotSame(first, third);
            Assert.Equal(2, counter.GetGraphCalls);
        }
    }

    internal sealed class RepositoryCallCounter
    {
        public int GetGraphCalls;
    }

    internal sealed class CountingExpertSystemRepository : IExpertSystemRepository
    {
        private readonly IExpertSystemRepository inner;
        private readonly RepositoryCallCounter counter;

        public CountingExpertSystemRepository(
            IExpertSystemRepository inner,
            RepositoryCallCounter counter)
        {
            this.inner = inner;
            this.counter = counter;
        }

        public Task<ExpertSystem?> GetGraphAsync(int id)
        {
            counter.GetGraphCalls++;
            return inner.GetGraphAsync(id);
        }

        public Task<List<ExpertSystem>> GetAllAsync(int limit, int page)
            => inner.GetAllAsync(limit, page);

        public Task<List<ExpertSystem>> GetAllAsync()
            => inner.GetAllAsync();

        public Task<ExpertSystem> GetByIdAsync(long id)
            => inner.GetByIdAsync(id);

        public Task UpdateAsync(ExpertSystem entity)
            => inner.UpdateAsync(entity);

        public Task UpdateAllAsync(List<ExpertSystem> entities)
            => inner.UpdateAllAsync(entities);

        public Task AddAsync(ExpertSystem entity)
            => inner.AddAsync(entity);

        public Task DeleteAsync(long id)
            => inner.DeleteAsync(id);

        public Task<int> GetCountAsync()
            => inner.GetCountAsync();

        public Task<ExpertSystem?> GetGraphByNameAsync(string name)
            => inner.GetGraphByNameAsync(name);

        public Task<List<ExpertSystemListRow>> GetListAsync()
            => inner.GetListAsync();

        public Task<List<Fact>> GetFactsAsync(int expertSystemId)
            => inner.GetFactsAsync(expertSystemId);
    }
}
