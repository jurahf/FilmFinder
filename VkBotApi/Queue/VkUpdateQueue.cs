using System.Threading.Channels;
using VkBotApi.Contracts;

namespace VkBotApi.Queue;

/// <summary>Очередь обновлений, чтобы callback отвечал ВКонтакте сразу.</summary>
public sealed class VkUpdateQueue
{
    private readonly Channel<VkIncomingMessage> channel =
        Channel.CreateUnbounded<VkIncomingMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(VkIncomingMessage message, CancellationToken cancellationToken = default)
    {
        return channel.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<VkIncomingMessage> ReadAllAsync(CancellationToken cancellationToken)
    {
        return channel.Reader.ReadAllAsync(cancellationToken);
    }
}
