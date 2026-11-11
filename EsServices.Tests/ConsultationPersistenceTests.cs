using EsConsultationServices;
using EsDb;
using EsDb.Model;
using EsDb.Model.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CoreConsultationService = EsConsultationServices.Contracts.IConsultationService;

namespace EsServices.Tests
{
    public class ConsultationPersistenceTests
    {
        [Fact]
        public async Task StartCreatesPersistedSessionAndQuestion()
        {
            await using var host = await ConsultationTestHost.CreateAsync();
            var expertSystemId = await host.SeedExpertSystemAsync("Detective", VariableType.Detective);

            var session = await host.WithServiceAsync(service => service.StartAsync(expertSystemId));

            Assert.Equal("Q", session.Step.Question?.Name);
            Assert.False(session.Step.IsCompleted);
            Assert.Equal(1, await host.CountAsync<Session>());
            Assert.Equal(1, await host.CountAsync<Consultation>());
            Assert.Equal(2, await host.CountAsync<GoalStack>());
            Assert.Equal(0, await host.CountAsync<ConsultationAnswer>());
        }

        [Fact]
        public async Task RestartRestoresCurrentQuestion()
        {
            await using var host = await ConsultationTestHost.CreateAsync();
            var expertSystemId = await host.SeedExpertSystemAsync("Detective", VariableType.Detective);

            var session = await host.WithServiceAsync(service => service.StartAsync(expertSystemId));
            await host.RestartAsync();

            var step = await host.WithServiceAsync(service => service.GetCurrentAsync(session.SessionId));

            Assert.Equal("Q", step.Question?.Name);
            Assert.False(step.IsCompleted);
        }

        [Fact]
        public async Task AnswerIsPersistedAndResultSurvivesRestart()
        {
            await using var host = await ConsultationTestHost.CreateAsync();
            var expertSystemId = await host.SeedExpertSystemAsync("Detective", VariableType.Detective);

            var session = await host.WithServiceAsync(service => service.StartAsync(expertSystemId));

            var step = await host.WithServiceAsync(service =>
                service.AnswerAsync(session.SessionId, "Q", "Да"));

            Assert.NotNull(step.Result);
            Assert.False(step.Result!.IsUnknown);
            Assert.Equal("T", step.Result.VariableName);
            Assert.Equal("Да", step.Result.Value);

            var answer = await host.QueryAsync(db => db.ConsultationAnswers.SingleAsync());
            Assert.Equal("Да", answer.Value);
            Assert.Equal(0, answer.Order);

            var finalSolution = await host.QueryAsync(db => db.FinalSolutions.SingleAsync());
            Assert.Equal("T", finalSolution.VariableName);
            Assert.Equal("Да", finalSolution.Value);

            await host.RestartAsync();

            var restored = await host.WithServiceAsync(service =>
                service.GetCurrentAsync(session.SessionId));

            Assert.NotNull(restored.Result);
            Assert.False(restored.Result!.IsUnknown);
            Assert.Equal("Да", restored.Result.Value);
        }

        [Fact]
        public async Task UnknownResultPersistsUnknownVariables()
        {
            await using var host = await ConsultationTestHost.CreateAsync();
            var expertSystemId = await host.SeedExpertSystemAsync("Detective", VariableType.Detective);

            var session = await host.WithServiceAsync(service => service.StartAsync(expertSystemId));

            var step = await host.WithServiceAsync(service =>
                service.AnswerAsync(session.SessionId, "Q", "Нет"));

            Assert.NotNull(step.Result);
            Assert.True(step.Result!.IsUnknown);

            var unknown = await host.QueryAsync(db => db.ConsultationUnknownVariables
                .Include(x => x.Variable)
                .SingleAsync());

            Assert.Equal("T", unknown.Variable.Name);

            await host.RestartAsync();

            var restored = await host.WithServiceAsync(service =>
                service.GetCurrentAsync(session.SessionId));

            Assert.NotNull(restored.Result);
            Assert.True(restored.Result!.IsUnknown);
            Assert.Contains("T", restored.Result.Explanation.UnknownVariables);
        }

        [Fact]
        public async Task AnswerWithoutFactIsSavedAndQuestionIsNotRepeated()
        {
            await using var host = await ConsultationTestHost.CreateAsync();
            var expertSystemId = await host.SeedExpertSystemAsync(
                "QueriableDetective",
                VariableType.QuerriableDetective);

            var session = await host.WithServiceAsync(service => service.StartAsync(expertSystemId));
            Assert.Equal("Q", session.Step.Question?.Name);

            // Для Q = "Нет" в ЭС нет записи Fact — ответ должен сохраниться отдельно.
            var step = await host.WithServiceAsync(service =>
                service.AnswerAsync(session.SessionId, "Q", "Нет"));

            Assert.Equal("T", step.Question?.Name);

            await host.RestartAsync();

            var restored = await host.WithServiceAsync(service =>
                service.GetCurrentAsync(session.SessionId));

            Assert.Equal("T", restored.Question?.Name);

            var result = await host.WithServiceAsync(service =>
                service.AnswerAsync(session.SessionId, "T", "Нет"));

            Assert.NotNull(result.Result);
            Assert.False(result.Result!.IsUnknown);
            Assert.Equal("Нет", result.Result.Value);
        }
    }

    internal sealed class ConsultationTestHost : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private ServiceProvider provider;

        private ConsultationTestHost(SqliteConnection connection, ServiceProvider provider)
        {
            this.connection = connection;
            this.provider = provider;
        }

        public static async Task<ConsultationTestHost> CreateAsync()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            await connection.OpenAsync();

            var provider = BuildProvider(connection);

            await using (var scope = provider.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EsDbContext>();
                await db.Database.EnsureCreatedAsync();
            }

            return new ConsultationTestHost(connection, provider);
        }

        public async Task RestartAsync()
        {
            await provider.DisposeAsync();
            provider = BuildProvider(connection);
        }

        public async Task<int> SeedExpertSystemAsync(string name, VariableType targetType)
        {
            return await QueryAsync(db =>
                ConsultationTestData.SeedExpertSystemAsync(db, name, targetType));
        }

        public async Task<T> WithServiceAsync<T>(
            Func<CoreConsultationService, Task<T>> action)
        {
            await using var scope = provider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<CoreConsultationService>();
            return await action(service);
        }

        public async Task<T> QueryAsync<T>(Func<EsDbContext, Task<T>> query)
        {
            await using var scope = provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<EsDbContext>();
            return await query(db);
        }

        public async Task<int> CountAsync<TEntity>() where TEntity : class
        {
            return await QueryAsync(db => db.Set<TEntity>().CountAsync());
        }

        public async ValueTask DisposeAsync()
        {
            await provider.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static ServiceProvider BuildProvider(SqliteConnection connection)
        {
            var services = new ServiceCollection();
            services.AddDbContext<EsDbContext>(options => options.UseSqlite(connection));
            services.AddEsConsultationServices();
            return services.BuildServiceProvider();
        }
    }
}
