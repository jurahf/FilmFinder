using TL;
using WTelegram;

namespace TgBotWClient.Bot;

/// <summary>
/// Отправляет сообщения через WTelegramClient в личный чат с пользователем.
/// Длинный текст разбивается на части, клавиатура ставится на последнюю.
/// </summary>
public sealed class TgMessageSender : ITgMessageSender
{
    // Запас под лимит Telegram в 4096 символов и возможные служебные символы.
    private const int MaxTextLength = 4000;

    private readonly Client client;
    private readonly ILogger<TgMessageSender> logger;

    public TgMessageSender(Client client, ILogger<TgMessageSender> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task SendTextAsync(
        long chatId,
        string text,
        ReplyMarkup? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var peer = new InputPeerUser(chatId, 0);
        var chunks = SplitText(text);

        for (var index = 0; index < chunks.Count; index++)
        {
            var isLast = index == chunks.Count - 1;
            await client.Messages_SendMessage(
                peer,
                chunks[index],
                Helpers.RandomLong(),
                reply_markup: isLast ? keyboard : null);
        }
    }

    public async Task SendPhotoAsync(
        long chatId,
        string photoUrl,
        ReplyMarkup? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            logger.LogWarning("Постер {PhotoUrl} не является абсолютной http(s)-ссылкой.", photoUrl);
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await client.Messages_SendMedia(
                new InputPeerUser(chatId, 0),
                new InputMediaPhotoExternal { url = uri.ToString() },
                string.Empty,
                Helpers.RandomLong(),
                reply_markup: keyboard);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Постер не должен мешать показать текстовую карточку.
            logger.LogWarning(exception, "Не удалось отправить постер {PhotoUrl}.", photoUrl);
        }
    }

    public async Task AnswerCallbackAsync(
        long callbackQueryId,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await client.Messages_SetBotCallbackAnswer(
            callbackQueryId,
            cache_time: 0,
            message: message);
    }

    private static IReadOnlyList<string> SplitText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new[] { string.Empty };

        if (text.Length <= MaxTextLength)
            return new[] { text };

        var chunks = new List<string>();
        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(MaxTextLength, text.Length - start);

            if (start + length < text.Length)
            {
                var breakAt = text.LastIndexOf('\n', start + length - 1, length);
                if (breakAt > start)
                    length = breakAt - start + 1;
            }

            chunks.Add(text.Substring(start, length));
            start += length;
        }

        return chunks;
    }
}
