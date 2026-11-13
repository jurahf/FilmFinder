using EsConsultationServices.Workflow;
using VkBotApi.Contracts;

namespace VkBotApi.Bot.Commands;

public sealed class StartConsultationCommand : IVkCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly VkStepPresenter presenter;

    public StartConsultationCommand(IConsultationWorkflow workflow, VkStepPresenter presenter)
    {
        this.workflow = workflow;
        this.presenter = presenter;
    }

    public string Name => "Consultation";

    public async Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await workflow.StartAsync(null, cancellationToken);
        await presenter.SendAsync(message.PeerId, result, cancellationToken);
    }
}
