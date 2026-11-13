using TL;

namespace TgBotWClient.Bot;

/// <summary>Отправка сообщений в Telegram и подтверждение callback-запросов.</summary>
public interface ITgMessageSender
{
    Task SendTextAsync(
        long chatId,
        string text,
        ReplyMarkup? keyboard = null,
        CancellationToken cancellationToken = default);

    Task SendPhotoAsync(
        long chatId,
        string photoUrl,
        ReplyMarkup? keyboard = null,
        CancellationToken cancellationToken = default);

    Task AnswerCallbackAsync(
        long callbackQueryId,
        string? message = null,
        CancellationToken cancellationToken = default);
}
