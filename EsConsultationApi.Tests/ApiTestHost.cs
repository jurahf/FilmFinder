using EsConsultationApi.Options;
using EsConsultationApi.Services;
using EsConsultationServices;
using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsDb;
using FilmDb;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EsConsultationApi.Tests;

internal sealed class ApiTestHost : IAsyncDisposable
{
    private readonly SqliteConnection esConnection;
    private readonly SqliteConnection filmsConnection;
    private ServiceProvider provider;

    private ApiTestHost(
        SqliteConnection esConnection,
        SqliteConnection filmsConnection,
        ServiceProvider provider)
    {
        this.esConnection = esConnection;
        this.filmsConnection = filmsConnection;
        this.provider = provider;
    }

    public static async Task<ApiTestHost> CreateAsync()
    {
        var esConnection = new SqliteConnection("Filename=:memory:");
        await esConnection.OpenAsync();

        var filmsConnection = new SqliteConnection("Filename=:memory:");
        await filmsConnection.OpenAsync();

        var provider = BuildProvider(esConnection, filmsConnection);

        await using (var scope = provider.CreateAsyncScope())
        {
            var esDb = scope.ServiceProvider.GetRequiredService<EsDbContext>();
            var filmsDb = scope.ServiceProvider.GetRequiredService<FilmDbContext>();
            await esDb.Database.EnsureCreatedAsync();
            await filmsDb.Database.EnsureCreatedAsync();
        }

        return new ApiTestHost(esConnection, filmsConnection, provider);
    }

    /// <summary>Имитирует перезапуск API: новый DI-контейнер на тех же базах.</summary>
    public async Task RestartAsync()
    {
        await provider.DisposeAsync();
        provider = BuildProvider(esConnection, filmsConnection);
    }

    public async Task<T> WithWorkflowAsync<T>(Func<ConsultationWorkflow, Task<T>> action)
    {
        await using var scope = provider.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<ConsultationWorkflow>());
    }

    public async Task WithWorkflowAsync(Func<ConsultationWorkflow, Task> action)
    {
        await using var scope = provider.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<ConsultationWorkflow>());
    }

    public async Task<T> WithServiceAsync<TService, T>(Func<TService, Task<T>> action)
        where TService : notnull
    {
        await using var scope = provider.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public async Task<T> QueryEsAsync<T>(Func<EsDbContext, Task<T>> query)
    {
        await using var scope = provider.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<EsDbContext>());
    }

    public async Task<T> QueryFilmsAsync<T>(Func<FilmDbContext, Task<T>> query)
    {
        await using var scope = provider.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<FilmDbContext>());
    }

    public async Task<FilmSeed> SeedFilmsAsync(bool withProperties = true)
    {
        return await QueryFilmsAsync(db => TestData.SeedFilmsAsync(db, withProperties));
    }

    public async Task<int> SeedExpertSystemAsync()
    {
        return await QueryEsAsync(TestData.SeedExpertSystemAsync);
    }

    public async ValueTask DisposeAsync()
    {
        await provider.DisposeAsync();
        await esConnection.DisposeAsync();
        await filmsConnection.DisposeAsync();
    }

    private static ServiceProvider BuildProvider(
        SqliteConnection esConnection,
        SqliteConnection filmsConnection)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddOptions();
        services.Configure<ConsultationOptions>(options =>
        {
            options.ExpertSystemId = 0;
            options.ExpertSystemName = "Фильмы";
        });

        services.AddDbContext<EsDbContext>(options => options.UseSqlite(esConnection));
        services.AddDbContext<FilmDbContext>(options => options.UseSqlite(filmsConnection));

        services.AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>();
        services.AddEsConsultationServices();

        services.AddScoped<ConsultationExpertSystemResolver>();
        services.AddScoped<ConsultationShownFilmStore>();
        services.AddScoped<ConsultationReviewStore>();
        services.AddScoped<ConsultationWorkflow>();

        return services.BuildServiceProvider();
    }
}
