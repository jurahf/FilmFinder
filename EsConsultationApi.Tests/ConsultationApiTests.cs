using EsConsultationApi.Mapping;
using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsConsultationServices.Workflow;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EsConsultationApi.Tests;

public class ConsultationWorkflowTests
{
    [Fact]
    public async Task Start_returns_session_and_first_question()
    {
        await using var host = await ApiTestHost.CreateAsync();
        await host.SeedExpertSystemAsync();

        var response = await host.WithWorkflowAsync(workflow => workflow.StartAsync(null));

        Assert.False(string.IsNullOrWhiteSpace(response.SessionId));
        Assert.Equal(ConsultationFlowStatus.Question, response.Status);
        Assert.Equal("Фильмы", response.ExpertSystemName);
        Assert.Equal("Q", response.Step.Question?.Name);
        Assert.Equal("Вопрос?", response.Step.Question?.Text);
        Assert.Null(response.Step.Result);
        Assert.Null(response.Film);
    }

    [Fact]
    public async Task Answer_completes_and_returns_result_with_film()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var films = await host.SeedFilmsAsync();
        await host.SeedExpertSystemAsync();

        var started = await host.WithWorkflowAsync(workflow => workflow.StartAsync(null));
        var response = await host.WithWorkflowAsync(workflow =>
            workflow.AnswerAsync(started.SessionId, "Q", "Да"));

        Assert.Equal(ConsultationFlowStatus.Completed, response.Status);
        Assert.NotNull(response.Step.Result);
        Assert.Equal(TestData.AdviceKey.ToString("D"), response.Step.Result!.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Step.Result.Display));
        Assert.Contains("Фильм A", response.Step.Result.Display);

        Assert.NotNull(response.Film);
        Assert.Equal(films.A, response.Film!.Id);

        var shownCount = await host.QueryEsAsync(db => db.ConsultationShownFilms.CountAsync());
        Assert.Equal(1, shownCount);
    }

    [Fact]
    public async Task Another_returns_next_film()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var films = await host.SeedFilmsAsync();
        await host.SeedExpertSystemAsync();

        var started = await host.WithWorkflowAsync(workflow => workflow.StartAsync(null));
        var first = await host.WithWorkflowAsync(workflow =>
            workflow.AnswerAsync(started.SessionId, "Q", "Да"));
        var second = await host.WithWorkflowAsync(workflow => workflow.GetAnotherAsync(started.SessionId));
        var third = await host.WithWorkflowAsync(workflow => workflow.GetAnotherAsync(started.SessionId));

        Assert.Equal(films.A, first.Film?.Id);
        Assert.Equal(films.B, second.Film?.Id);
        Assert.Contains(third.Film!.Id, films.Fallback);

        var ids = new[] { first.Film!.Id, second.Film!.Id, third.Film!.Id };
        Assert.Equal(3, ids.Distinct().Count());
    }

    [Fact]
    public async Task Another_after_restart_does_not_repeat()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var films = await host.SeedFilmsAsync();
        await host.SeedExpertSystemAsync();

        var started = await host.WithWorkflowAsync(workflow => workflow.StartAsync(null));
        await host.WithWorkflowAsync(workflow => workflow.AnswerAsync(started.SessionId, "Q", "Да"));
        await host.WithWorkflowAsync(workflow => workflow.GetAnotherAsync(started.SessionId));

        await host.RestartAsync();

        var next = await host.WithWorkflowAsync(workflow => workflow.GetAnotherAsync(started.SessionId));

        Assert.NotNull(next.Film);
        Assert.NotEqual(films.A, next.Film!.Id);
        Assert.NotEqual(films.B, next.Film.Id);
        Assert.Contains(next.Film.Id, films.Fallback);
    }

    [Fact]
    public async Task Another_when_candidates_exhausted_returns_not_found()
    {
        await using var host = await ApiTestHost.CreateAsync();
        await host.SeedFilmsAsync(withProperties: false);
        await host.SeedExpertSystemAsync();

        var started = await host.WithWorkflowAsync(workflow => workflow.StartAsync(null));
        await host.WithWorkflowAsync(workflow => workflow.AnswerAsync(started.SessionId, "Q", "Да"));
        await host.WithWorkflowAsync(workflow => workflow.GetAnotherAsync(started.SessionId));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            host.WithWorkflowAsync(workflow => workflow.GetAnotherAsync(started.SessionId)));
    }

    [Fact]
    public async Task Review_saves_estimate()
    {
        await using var host = await ApiTestHost.CreateAsync();
        await host.SeedFilmsAsync();
        await host.SeedExpertSystemAsync();

        var started = await host.WithWorkflowAsync(workflow => workflow.StartAsync(null));

        await host.WithWorkflowAsync(workflow => workflow.ReviewAsync(started.SessionId, true, "Отлично"));
        await host.WithWorkflowAsync(workflow => workflow.ReviewAsync(started.SessionId, false, null));

        var reviews = await host.QueryEsAsync(db => db.Reviews
            .OrderBy(x => x.Id)
            .ToListAsync());

        Assert.Equal(2, reviews.Count);
        Assert.Equal(10, reviews[0].Estimate);
        Assert.Equal("Отлично", reviews[0].Comment);
        Assert.Equal(0, reviews[1].Estimate);
        Assert.Null(reviews[1].Comment);
    }
}

public class FilmRecommendationTests{
    [Fact]
    public async Task FilmRecommendation_returns_explicit_advice_film_first()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var films = await host.SeedFilmsAsync();

        var film = await host.WithServiceAsync<IFilmRecommendationService, RecommendedFilm?>(service =>
            service.FindByAdviceAsync(TestData.AdviceKey));

        Assert.NotNull(film);
        Assert.Equal(films.A, film!.Id);
        Assert.Equal("Фильм A", film.Name);
    }

    [Fact]
    public async Task FilmRecommendation_falls_back_to_similar_films()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var films = await host.SeedFilmsAsync();

        var film = await host.WithServiceAsync<IFilmRecommendationService, RecommendedFilm?>(service =>
            service.FindByAdviceAsync(TestData.AdviceKey, new[] { films.A, films.B }));

        Assert.NotNull(film);
        Assert.Contains(film!.Id, films.Fallback);
    }

    [Fact]
    public async Task FilmRecommendation_excludes_shown_films()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var films = await host.SeedFilmsAsync();

        var film = await host.WithServiceAsync<IFilmRecommendationService, RecommendedFilm?>(service =>
            service.FindByAdviceAsync(TestData.AdviceKey, new[] { films.A }));

        Assert.Equal(films.B, film?.Id);
    }

    [Fact]
    public async Task AdviceDisplayProvider_returns_human_readable_description()
    {
        await using var host = await ApiTestHost.CreateAsync();
        await host.SeedFilmsAsync();

        var values = await host.WithServiceAsync<IConsultationDisplayProvider, IReadOnlyList<ConsultationDisplayValue>>(
            service => service.GetDisplayValuesAsync());

        var value = Assert.Single(values);
        Assert.Equal(AdviceCatalogConstants.DomainName, value.DomainName);
        Assert.Equal(TestData.AdviceKey.ToString("D"), value.Value);
        Assert.Contains("Фильм A", value.Display);
        Assert.Contains("НЕ Мрачное", value.Display);
    }
}

public class ConsultationResponseMapperTests
{
    [Fact]
    public void Maps_status_and_film()
    {
        var adviceKey = Guid.NewGuid().ToString("D");
        var result = new ConsultationFlowResult
        {
            SessionId = "session",
            ExpertSystemId = 7,
            ExpertSystemName = "Фильмы",
            Status = ConsultationFlowStatus.Completed,
            Step = new ConsultationStep
            {
                Result = new ConsultationResult
                {
                    VariableName = "Совет",
                    Value = adviceKey,
                    Display = "Описание совета"
                }
            },
            Film = new RecommendedFilm
            {
                Id = 42,
                Name = "Фильм",
                Year = 2001,
                Rating = 8.5m,
                Tags = new[] { new FilmTag { Name = "Смешное", Percent = 80 } }
            }
        };

        var response = ConsultationResponseMapper.ToResponse(result);

        Assert.Equal("session", response.SessionId);
        Assert.Equal("Completed", response.Status);
        Assert.Equal(adviceKey, response.Result?.Value);
        Assert.Equal(42, response.Film?.Id);
        Assert.Equal("Фильм", response.Film?.Name);
        Assert.Equal("Смешное", Assert.Single(response.Film!.Tags).Name);
    }
}
