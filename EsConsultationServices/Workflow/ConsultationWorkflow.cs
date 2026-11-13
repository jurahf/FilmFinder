using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsDb;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationServices.Workflow;

/// <summary>
/// Общий сценарий консультации: связывает движок, подбор фильма
/// и хранение показов/оценок. Не зависит от HTTP или VK.
/// </summary>
public sealed class ConsultationWorkflow : IConsultationWorkflow
{
    private const int ShowAttempts = 3;

    private readonly IConsultationService consultation;
    private readonly IFilmRecommendationService films;
    private readonly ConsultationExpertSystemResolver expertSystemResolver;
    private readonly ConsultationShownFilmStore shownFilms;
    private readonly ConsultationReviewStore reviews;
    private readonly EsDbContext db;

    public ConsultationWorkflow(
        IConsultationService consultation,
        IFilmRecommendationService films,
        ConsultationExpertSystemResolver expertSystemResolver,
        ConsultationShownFilmStore shownFilms,
        ConsultationReviewStore reviews,
        EsDbContext db)
    {
        this.consultation = consultation;
        this.films = films;
        this.expertSystemResolver = expertSystemResolver;
        this.shownFilms = shownFilms;
        this.reviews = reviews;
        this.db = db;
    }

    public async Task<ConsultationFlowResult> StartAsync(
        int? expertSystemId,
        CancellationToken cancellationToken = default)
    {
        var id = await expertSystemResolver.ResolveAsync(expertSystemId, cancellationToken);
        var session = await consultation.StartAsync(id, cancellationToken);

        return CreateResult(
            session.SessionId,
            session.ExpertSystemId,
            session.ExpertSystemName,
            session.Step);
    }

    public async Task<ConsultationFlowResult> GetCurrentAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var step = await consultation.GetCurrentAsync(sessionId, cancellationToken);
        var expertSystem = await GetExpertSystemAsync(sessionId, cancellationToken);

        return CreateResult(sessionId, expertSystem.Id, expertSystem.Name, step);
    }

    public async Task<ConsultationFlowResult> AnswerAsync(
        string sessionId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default)
    {
        var step = await consultation.AnswerAsync(sessionId, variableName, value, cancellationToken);
        var expertSystem = await GetExpertSystemAsync(sessionId, cancellationToken);

        RecommendedFilm? film = null;
        var adviceKey = GetAdviceKey(step);
        if (adviceKey.HasValue)
            film = await FindAndRegisterFilmAsync(sessionId, adviceKey.Value, cancellationToken);

        return CreateResult(sessionId, expertSystem.Id, expertSystem.Name, step, film);
    }

    public async Task<ConsultationFlowResult> GetAnotherAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var step = await consultation.GetCurrentAsync(sessionId, cancellationToken);
        var expertSystem = await GetExpertSystemAsync(sessionId, cancellationToken);

        var adviceKey = GetAdviceKey(step)
            ?? throw new InvalidOperationException(
                step.Result == null
                    ? "Консультация ещё не завершена: сначала ответьте на вопросы."
                    : "Результат консультации не является советом: другой фильм подобрать нельзя.");

        var film = await FindAndRegisterFilmAsync(sessionId, adviceKey, cancellationToken)
            ?? throw new KeyNotFoundException(
                "Больше нет подходящих фильмов: все рекомендации по этому совету уже показаны.");

        return CreateResult(sessionId, expertSystem.Id, expertSystem.Name, step, film);
    }

    public async Task ReviewAsync(
        string sessionId,
        bool liked,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        await reviews.AddAsync(sessionId, liked, comment, cancellationToken);
    }

    private async Task<RecommendedFilm?> FindAndRegisterFilmAsync(
        string sessionId,
        Guid adviceKey,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < ShowAttempts; attempt++)
        {
            var shown = await shownFilms.GetIdsAsync(sessionId, cancellationToken);
            var film = await films.FindByAdviceAsync(adviceKey, shown, cancellationToken);

            if (film == null)
                return null;

            if (await shownFilms.TryAddAsync(sessionId, film.Id, cancellationToken))
                return film;
        }

        throw new InvalidOperationException(
            "Не удалось зафиксировать показ фильма: показ изменился параллельно. Повторите запрос.");
    }

    private async Task<ExpertSystemInfo> GetExpertSystemAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var info = await db.Consultations
            .AsNoTracking()
            .Where(x => x.Session.SessionId == sessionId)
            .Select(x => new ExpertSystemInfo(x.ExpertSystemId, x.ExpertSystem.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return info
            ?? throw new KeyNotFoundException($"Консультация с идентификатором сессии '{sessionId}' не найдена.");
    }

    private static Guid? GetAdviceKey(ConsultationStep step)
    {
        if (step.Result == null || step.Result.IsUnknown || string.IsNullOrWhiteSpace(step.Result.Value))
            return null;

        return Guid.TryParse(step.Result.Value, out var adviceKey) ? adviceKey : null;
    }

    private static ConsultationFlowResult CreateResult(
        string sessionId,
        int expertSystemId,
        string expertSystemName,
        ConsultationStep step,
        RecommendedFilm? film = null)
    {
        return new ConsultationFlowResult
        {
            SessionId = sessionId,
            ExpertSystemId = expertSystemId,
            ExpertSystemName = expertSystemName,
            Status = GetStatus(step),
            Step = step,
            Film = film
        };
    }

    private static ConsultationFlowStatus GetStatus(ConsultationStep step)
    {
        if (step.Result == null)
            return ConsultationFlowStatus.Question;

        return step.Result.IsUnknown
            ? ConsultationFlowStatus.Unknown
            : ConsultationFlowStatus.Completed;
    }

    private sealed record ExpertSystemInfo(int Id, string Name);
}
