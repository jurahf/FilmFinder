namespace TgBotWClient.Bot;

/// <summary>
/// Привязка чата Telegram к текущей консультации.
/// Хранится в EsDb.FrontendChatSessions, чтобы пережить перезапуск сервиса.
/// </summary>
public interface ITgChatSessionStore
{
    Task<string?> GetSessionIdAsync(long chatId, CancellationToken cancellationToken = default);

    Task SetSessionIdAsync(long chatId, string sessionId, CancellationToken cancellationToken = default);

    Task ClearAsync(long chatId, CancellationToken cancellationToken = default);
}
