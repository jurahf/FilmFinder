using System.Globalization;
using EsDb;
using EsDb.Model;
using Microsoft.EntityFrameworkCore;

namespace TgBotWClient.Bot;

/// <summary>
/// Хранит привязку «чат Telegram → консультация» в EsDb.FrontendChatSessions.
/// </summary>
public sealed class TgChatSessionStore : ITgChatSessionStore
{
    public const string Frontend = "telegram";

    private readonly EsDbContext db;
    private readonly ILogger<TgChatSessionStore> logger;

    public TgChatSessionStore(EsDbContext db, ILogger<TgChatSessionStore> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public async Task<string?> GetSessionIdAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var key = FormatChatId(chatId);

        return await db.FrontendChatSessions
            .AsNoTracking()
            .Where(x => x.Frontend == Frontend && x.ChatId == key)
            .Select(x => x.SessionId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetSessionIdAsync(
        long chatId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var key = FormatChatId(chatId);
        await UpsertAsync(key, sessionId, cancellationToken);
    }

    public async Task ClearAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var key = FormatChatId(chatId);

        var entity = await db.FrontendChatSessions
            .FirstOrDefaultAsync(x => x.Frontend == Frontend && x.ChatId == key, cancellationToken);

        if (entity == null)
            return;

        db.FrontendChatSessions.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Привязка чата {ChatId} к консультации удалена.", chatId);
    }

    private async Task UpsertAsync(string chatId, string sessionId, CancellationToken cancellationToken)
    {
        var entity = await db.FrontendChatSessions
            .FirstOrDefaultAsync(x => x.Frontend == Frontend && x.ChatId == chatId, cancellationToken);

        if (entity == null)
        {
            db.FrontendChatSessions.Add(new FrontendChatSession
            {
                Frontend = Frontend,
                ChatId = chatId,
                SessionId = sessionId,
                UpdateDate = DateTime.UtcNow
            });
        }
        else
        {
            entity.SessionId = sessionId;
            entity.UpdateDate = DateTime.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            // Гонка по уникальному индексу (Frontend, ChatId): строку уже добавил другой scope.
            logger.LogWarning(exception, "Не удалось сохранить привязку чата {ChatId}, повторяю.", chatId);

            db.ChangeTracker.Clear();
            var existing = await db.FrontendChatSessions
                .FirstOrDefaultAsync(x => x.Frontend == Frontend && x.ChatId == chatId, cancellationToken);

            if (existing == null)
                throw;

            existing.SessionId = sessionId;
            existing.UpdateDate = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static string FormatChatId(long chatId)
    {
        return chatId.ToString(CultureInfo.InvariantCulture);
    }
}
