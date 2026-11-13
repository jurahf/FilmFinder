using TgBotWClient.Contracts;

namespace TgBotWClient.Bot.Commands;

public interface ITgCommand
{
    string Name { get; }

    Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken);
}
