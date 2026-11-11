using EsConsultationApi.Contracts;
using EsConsultationApi.Mapping;
using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsDb;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationApi.Services;

/// <summary>
/// Бизнес-логика HTTP-слоя: связывает движок консультации, подбор фильма
/// и хранение показов/оценок.
/// </summary>
public sealed class ConsultationWorkflow
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

    public async Task<ConsultationResponse> StartAsync(
        int? expertSystemId,
        CancellationToken cancellationToken = default)
    {
        var id = await expertSystemResolver.ResolveAsync(expertSystemId, cancellationToken);
        var session = await consultation.StartAsync(id, cancellationToken);

        return CreateResponse(
            session.SessionId,
            session.ExpertSystemId,
            session.ExpertSystemName,
            session.Step);
    }

    public async Task<ConsultationResponse> GetCurrentAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var step = await consultation.GetCurrentAsync(sessionId, cancellationToken);
        var expertSystem = await GetExpertSystemAsync(sessionId, cancellationToken);

        return CreateResponse(sessionId, expertSystem.Id, expertSystem.Name, step);
    }

    public async Task<ConsultationResponse> AnswerAsync(
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

        return CreateResponse(sessionId, expertSystem.Id, expertSystem.Name, step, film);
    }

    public async Task<ConsultationResponse> GetAnotherAsync(
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

        return CreateResponse(sessionId, expertSystem.Id, expertSystem.Name, step, film);
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

    private static ConsultationResponse CreateResponse(
        string sessionId,
        int expertSystemId,
        string expertSystemName,
        ConsultationStep step,
        RecommendedFilm? film = null)
    {
        var status = step.Result == null
            ? ConsultationStatuses.Question
            : step.Result.IsUnknown
                ? ConsultationStatuses.Unknown
                : ConsultationStatuses.Completed;

        return new ConsultationResponse
        {
            SessionId = sessionId,
            ExpertSystemId = expertSystemId,
            ExpertSystemName = expertSystemName,
            Status = status,
            Question = step.Question,
            Result = step.Result,
            Film = film == null ? null : FilmMapper.ToDto(film)
        };
    }

    private sealed record ExpertSystemInfo(int Id, string Name);
}
