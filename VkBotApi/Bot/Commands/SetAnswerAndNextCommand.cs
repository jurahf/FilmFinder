using EsConsultationServices.Workflow;
using VkBotApi.Contracts;

namespace VkBotApi.Bot.Commands;

public sealed class SetAnswerAndNextCommand : IVkCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly IVkMessageSender sender;
    private readonly VkStepPresenter presenter;

    public SetAnswerAndNextCommand(
        IConsultationWorkflow workflow,
        IVkMessageSender sender,
        VkStepPresenter presenter)
    {
        this.workflow = workflow;
        this.sender = sender;
        this.presenter = presenter;
    }

    public string Name => "SetAnswerAndNext";

    public async Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = request.GetArgument(0);
        var variableName = request.GetArgument(1);
        var value = request.GetArgument(2);

        if (string.IsNullOrWhiteSpace(sessionId) ||
            string.IsNullOrWhiteSpace(variableName) ||
            value == null)
        {
            throw new ArgumentException("Не хватает параметров команды SetAnswerAndNext.");
        }

        var result = await workflow.AnswerAsync(
            sessionId,
            variableName,
            value,
            cancellationToken);

        if (result.Status != ConsultationFlowStatus.Question)
        {
            await sender.SendTextAsync(
                message.PeerId,
                VkBotTexts.SearchingFilm,
                cancellationToken: cancellationToken);
        }

        await presenter.SendAsync(message.PeerId, result, cancellationToken);
    }
}
