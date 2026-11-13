using EsConsultationServices.Workflow;

namespace VkBotApi.Bot;

/// <summary>Показывает шаг workflow: вопрос, фильм или сообщение об отсутствии результата.</summary>
public sealed class VkStepPresenter
{
    private readonly IVkMessageSender sender;
    private readonly VkKeyboardFactory keyboards;
    private readonly VkFilmPresenter films;

    public VkStepPresenter(
        IVkMessageSender sender,
        VkKeyboardFactory keyboards,
        VkFilmPresenter films)
    {
        this.sender = sender;
        this.keyboards = keyboards;
        this.films = films;
    }

    public async Task SendAsync(
        long peerId,
        ConsultationFlowResult result,
        CancellationToken cancellationToken)
    {
        if (result.Status == ConsultationFlowStatus.Question &&
            result.Step.Question is { } question)
        {
            await sender.SendTextAsync(
                peerId,
                question.Text,
                keyboards.CreateQuestionKeyboard(result.SessionId, question),
                cancellationToken);
            return;
        }

        if (result.Film is { } film)
        {
            await films.SendAsync(peerId, result.SessionId, film, cancellationToken);
            return;
        }

        await sender.SendTextAsync(
            peerId,
            VkBotTexts.NotFound,
            keyboards.CreateRetryKeyboard(),
            cancellationToken);
    }
}
