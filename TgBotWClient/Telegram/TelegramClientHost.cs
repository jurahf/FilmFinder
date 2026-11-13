using System.Text;
using Microsoft.Extensions.Options;
using TgBotWClient.Contracts;
using TgBotWClient.Options;
using TL;
using WTelegram;

namespace TgBotWClient.Telegram;

/// <summary>
/// Авторизует бота, подписывается на обновления и кладёт их в очередь.
/// Вся обработка выполняется в <see cref="TgUpdateProcessor"/>.
/// </summary>
public sealed class TelegramClientHost : IHostedService
{
    private readonly Client client;
    private readonly TelegramOptions options;
    private readonly TgUpdateQueue queue;
    private readonly ILogger<TelegramClientHost> logger;

    public TelegramClientHost(
        Client client,
        IOptions<TelegramOptions> options,
        TgUpdateQueue queue,
        ILogger<TelegramClientHost> logger)
    {
        this.client = client;
        this.options = options.Value;
        this.queue = queue;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var user = await client.LoginBotIfNeeded(options.BotToken);

        logger.LogInformation(
            "Бот @{Username} (id {UserId}) авторизован в Telegram.",
            user.username,
            user.id);

        client.OnUpdates += HandleUpdatesAsync;
        logger.LogInformation("Подписка на обновления Telegram включена.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.OnUpdates -= HandleUpdatesAsync;
        logger.LogInformation("Подписка на обновления Telegram отключена.");
        return Task.CompletedTask;
    }

    private async Task HandleUpdatesAsync(UpdatesBase updates)
    {
        try
        {
            foreach (var update in updates.UpdateList)
            {
                switch (update)
                {
                    case UpdateNewMessage updateNew when updateNew.message is Message message:
                        await HandleMessageAsync(message);
                        break;

                    case UpdateBotCallbackQuery callback when callback.peer is PeerUser peer:
                        await HandleCallbackAsync(callback, peer);
                        break;
                }
            }
        }
        catch (Exception exception)
        {
            // Исключение не должно всплывать в клиент WTelegram.
            logger.LogError(exception, "Ошибка разбора обновления Telegram.");
        }
    }

    private async Task HandleMessageAsync(Message message)
    {
        if (message.flags.HasFlag(Message.Flags.out_))
            return;

        if (message.peer_id is not PeerUser peer)
            return;

        if (string.IsNullOrWhiteSpace(message.message))
            return;

        var userId = (message.from_id as PeerUser)?.user_id ?? peer.user_id;

        await queue.EnqueueAsync(new TgIncomingMessage
        {
            ChatId = peer.user_id,
            UserId = userId,
            MessageId = message.id,
            Text = message.message
        });
    }

    private async Task HandleCallbackAsync(UpdateBotCallbackQuery callback, PeerUser peer)
    {
        if (callback.data == null || callback.data.Length == 0)
            return;

        await queue.EnqueueAsync(new TgIncomingMessage
        {
            ChatId = peer.user_id,
            UserId = callback.user_id,
            MessageId = callback.msg_id,
            CallbackData = Encoding.UTF8.GetString(callback.data),
            CallbackQueryId = callback.query_id
        });
    }
}
