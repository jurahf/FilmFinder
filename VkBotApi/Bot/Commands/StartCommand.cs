using VkBotApi.Contracts;

namespace VkBotApi.Bot.Commands;

public sealed class StartCommand : IVkCommand
{
    private readonly IVkMessageSender sender;
    private readonly VkKeyboardFactory keyboards;

    public StartCommand(IVkMessageSender sender, VkKeyboardFactory keyboards)
    {
        this.sender = sender;
        this.keyboards = keyboards;
    }

    public string Name => "start";

    public Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken)
    {
        return sender.SendTextAsync(
            message.PeerId,
            VkBotTexts.Hello,
            keyboards.CreateConsultationKeyboard(),
            cancellationToken);
    }
}
