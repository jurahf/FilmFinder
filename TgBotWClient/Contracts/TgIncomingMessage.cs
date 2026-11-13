namespace TgBotWClient.Contracts;

/// <summary>
/// Входящее обновление Telegram, уже приведённое к виду, не зависящему от TL-схемы.
/// </summary>
public sealed class TgIncomingMessage
{
    public long ChatId { get; init; }
    public long UserId { get; init; }
    public long MessageId { get; init; }
    public string? Text { get; init; }
    public string? CallbackData { get; init; }
    public long CallbackQueryId { get; init; }

    public bool IsCallback => CallbackData != null;
}
