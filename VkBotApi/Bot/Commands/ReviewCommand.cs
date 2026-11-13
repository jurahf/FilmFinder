using EsConsultationServices.Workflow;
using VkBotApi.Contracts;

namespace VkBotApi.Bot.Commands;

public sealed class ReviewCommand : IVkCommand
{
    private const string Positive = "Positive";
    private const string Negative = "Negative";

    private readonly IConsultationWorkflow workflow;
    private readonly IVkMessageSender sender;
    private readonly VkKeyboardFactory keyboards;
    private readonly ILogger<ReviewCommand> logger;

    public ReviewCommand(
        IConsultationWorkflow workflow,
        IVkMessageSender sender,
        VkKeyboardFactory keyboards,
        ILogger<ReviewCommand> logger)
    {
        this.workflow = workflow;
        this.sender = sender;
        this.keyboards = keyboards;
        this.logger = logger;
    }

    public string Name => "Review";

    public async Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = request.GetArgument(0);
        var estimation = request.GetArgument(1);

        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(estimation))
            throw new ArgumentException("Не хватает параметров команды Review.");

        var liked = string.Equals(estimation, Positive, StringComparison.OrdinalIgnoreCase);
        if (!liked && !string.Equals(estimation, Negative, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Нераспознанная оценка.");

        var canonicalEstimation = liked ? Positive : Negative;
        var comment = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} - {canonicalEstimation}";

        try
        {
            await workflow.ReviewAsync(sessionId, liked, comment, cancellationToken);
        }
        catch (KeyNotFoundException exception)
        {
            // Старый бот молча пропускал запись и всё равно благодарил.
            logger.LogWarning(exception, "Сессия {SessionId} для оценки не найдена.", sessionId);
        }

        await sender.SendTextAsync(
            message.PeerId,
            VkBotTexts.ThanksForReview,
            keyboards.CreateConsultationKeyboard(),
            cancellationToken);
    }
}
