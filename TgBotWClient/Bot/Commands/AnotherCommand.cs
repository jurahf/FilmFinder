using EsConsultationServices.Workflow;
using TgBotWClient.Bot;
using TgBotWClient.Contracts;

namespace TgBotWClient.Bot.Commands;

/// <summary>Показывает следующий фильм по той же консультации.</summary>
public sealed class AnotherCommand : ITgCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly ITgChatSessionStore chatSessions;
    private readonly ITgMessageSender sender;
    private readonly TgStepPresenter presenter;
    private readonly TgKeyboardFactory keyboards;

    public AnotherCommand(
        IConsultationWorkflow workflow,
        ITgChatSessionStore chatSessions,
        ITgMessageSender sender,
        TgStepPresenter presenter,
        TgKeyboardFactory keyboards)
    {
        this.workflow = workflow;
        this.chatSessions = chatSessions;
        this.sender = sender;
        this.presenter = presenter;
        this.keyboards = keyboards;
    }

    public string Name => TgCommandNames.Another;

    public async Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = await chatSessions.GetSessionIdAsync(message.ChatId, cancellationToken);
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.NoBinding,
                keyboards.CreateStartKeyboard(),
                cancellationToken);
            return;
        }

        await sender.SendTextAsync(
            message.ChatId,
            TgBotTexts.SearchingAnother,
            cancellationToken: cancellationToken);

        ConsultationFlowResult result;
        try
        {
            result = await workflow.GetAnotherAsync(sessionId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            await chatSessions.ClearAsync(message.ChatId, cancellationToken);

            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.NoMoreFilms,
                keyboards.CreateStartKeyboard(),
                cancellationToken);
            return;
        }
        catch (InvalidOperationException)
        {
            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.NotCompleted,
                keyboards.CreateRetryKeyboard(),
                cancellationToken);
            return;
        }

        await presenter.SendAsync(message.ChatId, result, cancellationToken);
    }
}
