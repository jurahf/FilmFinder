using System.Threading.Channels;
using TgBotWClient.Contracts;

namespace TgBotWClient.Telegram;

/// <summary>
/// Очередь обновлений, чтобы обработчик WTelegramClient освобождался сразу,
/// а бизнес-логика выполнялась последовательно в одном сервисе.
/// </summary>
public sealed class TgUpdateQueue
{
    private readonly Channel<TgIncomingMessage> channel =
        Channel.CreateUnbounded<TgIncomingMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(
        TgIncomingMessage message,
        CancellationToken cancellationToken = default)
    {
        return channel.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<TgIncomingMessage> ReadAllAsync(CancellationToken cancellationToken)
    {
        return channel.Reader.ReadAllAsync(cancellationToken);
    }
}
