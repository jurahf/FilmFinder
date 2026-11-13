using EsConsultationServices.Workflow;
using VkBotApi.Contracts;

namespace VkBotApi.Bot.Commands;

public sealed class AnotherCommand : IVkCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly IVkMessageSender sender;
    private readonly VkStepPresenter presenter;
    private readonly VkKeyboardFactory keyboards;

    public AnotherCommand(
        IConsultationWorkflow workflow,
        IVkMessageSender sender,
        VkStepPresenter presenter,
        VkKeyboardFactory keyboards)
    {
        this.workflow = workflow;
        this.sender = sender;
        this.presenter = presenter;
        this.keyboards = keyboards;
    }

    public string Name => "Another";

    public async Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = request.GetArgument(0);
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Не хватает параметра sessionId команды Another.");

        await sender.SendTextAsync(
            message.PeerId,
            VkBotTexts.SearchingAnother,
            cancellationToken: cancellationToken);

        ConsultationFlowResult result;
        try
        {
            result = await workflow.GetAnotherAsync(sessionId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            await sender.SendTextAsync(
                message.PeerId,
                VkBotTexts.NotFound,
                keyboards.CreateRetryKeyboard(),
                cancellationToken);
            return;
        }

        await presenter.SendAsync(message.PeerId, result, cancellationToken);
    }
}
