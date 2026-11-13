using VkBotApi.Contracts;

namespace VkBotApi.Bot.Commands;

public interface IVkCommand
{
    string Name { get; }

    Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken);
}
