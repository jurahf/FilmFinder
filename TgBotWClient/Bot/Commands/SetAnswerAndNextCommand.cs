using EsConsultationServices.Workflow;
using TgBotWClient.Bot;
using TgBotWClient.Contracts;

namespace TgBotWClient.Bot.Commands;

/// <summary>
/// Фиксирует ответ на вопрос. Callback data содержит sessionId, индекс варианта
/// и хеш вопроса, поэтому ответ на устаревший вопрос отклоняется.
/// </summary>
public sealed class SetAnswerAndNextCommand : ITgCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly ITgChatSessionStore chatSessions;
    private readonly ITgMessageSender sender;
    private readonly TgStepPresenter presenter;

    public SetAnswerAndNextCommand(
        IConsultationWorkflow workflow,
        ITgChatSessionStore chatSessions,
        ITgMessageSender sender,
        TgStepPresenter presenter)
    {
        this.workflow = workflow;
        this.chatSessions = chatSessions;
        this.sender = sender;
        this.presenter = presenter;
    }

    public string Name => TgCommandNames.Answer;

    public async Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = request.GetArgument(0);
        var indexText = request.GetArgument(1);
        var hash = request.GetArgument(2);

        if (string.IsNullOrWhiteSpace(sessionId) ||
            !int.TryParse(indexText, out var index) ||
            string.IsNullOrWhiteSpace(hash))
        {
            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.StaleQuestion,
                cancellationToken: cancellationToken);
            return;
        }

        var current = await workflow.GetCurrentAsync(sessionId, cancellationToken);
        var question = current.Step.Question;

        if (current.Status != ConsultationFlowStatus.Question ||
            question == null ||
            !string.Equals(
                TgKeyboardFactory.ComputeQuestionHash(question),
                hash,
                StringComparison.OrdinalIgnoreCase) ||
            index < 0 ||
            index >= question.Answers.Count)
        {
            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.StaleQuestion,
                cancellationToken: cancellationToken);

            await presenter.SendAsync(message.ChatId, current, cancellationToken);
            return;
        }

        var option = question.Answers[index];
        var result = await workflow.AnswerAsync(
            sessionId,
            question.Name,
            option.Value,
            cancellationToken);

        await chatSessions.SetSessionIdAsync(message.ChatId, result.SessionId, cancellationToken);

        if (result.Status != ConsultationFlowStatus.Question)
        {
            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.SearchingFilm,
                cancellationToken: cancellationToken);
        }

        await presenter.SendAsync(message.ChatId, result, cancellationToken);
    }
}
