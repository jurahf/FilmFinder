using TgBotWClient.Bot;
using TgBotWClient.Contracts;

namespace TgBotWClient.Bot.Commands;

public sealed class StartCommand : ITgCommand
{
    private readonly ITgMessageSender sender;
    private readonly TgKeyboardFactory keyboards;

    public StartCommand(ITgMessageSender sender, TgKeyboardFactory keyboards)
    {
        this.sender = sender;
        this.keyboards = keyboards;
    }

    public string Name => TgCommandNames.Start;

    public Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken)
    {
        return sender.SendTextAsync(
            message.ChatId,
            TgBotTexts.Hello,
            keyboards.CreateStartKeyboard(),
            cancellationToken);
    }
}
