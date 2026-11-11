using EsDb;
using EsDb.Model;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationApi.Services;

/// <summary>Сохраняет оценку консультации в EsDb.Reviews по SessionId.</summary>
public sealed class ConsultationReviewStore
{
    /// <summary>Оценка «понравилось».</summary>
    public const int LikedEstimate = 10;

    /// <summary>Оценка «не понравилось».</summary>
    public const int DislikedEstimate = 0;

    private readonly EsDbContext db;

    public ConsultationReviewStore(EsDbContext db)
    {
        this.db = db;
    }

    public async Task AddAsync(
        string sessionId,
        bool liked,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var session = await db.Sessions
            .FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Сессия '{sessionId}' не найдена.");

        db.Reviews.Add(new Review
        {
            SessionId = session.Id,
            Estimate = liked ? LikedEstimate : DislikedEstimate,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
