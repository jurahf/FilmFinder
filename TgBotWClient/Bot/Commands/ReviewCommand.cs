using EsConsultationServices.Workflow;
using TgBotWClient.Bot;
using TgBotWClient.Contracts;

namespace TgBotWClient.Bot.Commands;

/// <summary>Простая оценка «ок / не ок» для текущей консультации.</summary>
public sealed class ReviewCommand : ITgCommand
{
    private readonly IConsultationWorkflow workflow;
    private readonly ITgChatSessionStore chatSessions;
    private readonly ITgMessageSender sender;
    private readonly TgKeyboardFactory keyboards;
    private readonly ILogger<ReviewCommand> logger;

    public ReviewCommand(
        IConsultationWorkflow workflow,
        ITgChatSessionStore chatSessions,
        ITgMessageSender sender,
        TgKeyboardFactory keyboards,
        ILogger<ReviewCommand> logger)
    {
        this.workflow = workflow;
        this.chatSessions = chatSessions;
        this.sender = sender;
        this.keyboards = keyboards;
        this.logger = logger;
    }

    public string Name => TgCommandNames.Review;

    public async Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken)
    {
        var estimation = request.GetArgument(0) ?? request.GetArgument(1);
        var liked = string.Equals(estimation, TgEstimation.Positive, StringComparison.OrdinalIgnoreCase);

        if (!liked &&
            !string.Equals(estimation, TgEstimation.Negative, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Нераспознанная оценка фильма.");
        }

        var canonicalEstimation = liked ? TgEstimation.Positive : TgEstimation.Negative;
        var comment = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} - {canonicalEstimation}";

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

        try
        {
            await workflow.ReviewAsync(sessionId, liked, comment, cancellationToken);
        }
        catch (KeyNotFoundException exception)
        {
            // Как и старый бот: запись пропускается, пользователь всё равно получает благодарность.
            logger.LogWarning(exception, "Сессия {SessionId} для оценки не найдена.", sessionId);
            await chatSessions.ClearAsync(message.ChatId, cancellationToken);
        }

        await sender.SendTextAsync(
            message.ChatId,
            TgBotTexts.ThanksForReview,
            keyboards.CreateFilmKeyboard(),
            cancellationToken);
    }
}
