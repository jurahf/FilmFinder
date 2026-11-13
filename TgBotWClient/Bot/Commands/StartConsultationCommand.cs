using EsConsultationServices.Workflow;
using TgBotWClient.Bot;
using TgBotWClient.Contracts;

namespace TgBotWClient.Bot.Commands;

public sealed class StartConsultationCommand : ITgCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly ITgChatSessionStore chatSessions;
    private readonly TgStepPresenter presenter;

    public StartConsultationCommand(
        IConsultationWorkflow workflow,
        ITgChatSessionStore chatSessions,
        TgStepPresenter presenter)
    {
        this.workflow = workflow;
        this.chatSessions = chatSessions;
        this.presenter = presenter;
    }

    public string Name => TgCommandNames.Consultation;

    public async Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await workflow.StartAsync(null, cancellationToken);

        await chatSessions.SetSessionIdAsync(
            message.ChatId,
            result.SessionId,
            cancellationToken);

        await presenter.SendStartedAsync(message.ChatId, result, cancellationToken);
    }
}
