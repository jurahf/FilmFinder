using EsConsultationServices.Workflow;

namespace TgBotWClient.Bot;

/// <summary>Показывает шаг консультации: вопрос, фильм или отсутствие результата.</summary>
public sealed class TgStepPresenter
{
    private readonly ITgMessageSender sender;
    private readonly TgKeyboardFactory keyboards;
    private readonly TgFilmPresenter films;

    public TgStepPresenter(
        ITgMessageSender sender,
        TgKeyboardFactory keyboards,
        TgFilmPresenter films)
    {
        this.sender = sender;
        this.keyboards = keyboards;
        this.films = films;
    }

    /// <summary>
    /// Начало консультации: сначала обновляем reply-клавиатуру отдельным сообщением
    /// (на одном сообщении допустима только одна клавиатура), затем показываем шаг.
    /// </summary>
    public async Task SendStartedAsync(
        long chatId,
        ConsultationFlowResult result,
        CancellationToken cancellationToken)
    {
        await sender.SendTextAsync(
            chatId,
            TgBotTexts.Started,
            keyboards.CreateQuestionKeyboard(),
            cancellationToken);

        await SendAsync(chatId, result, cancellationToken);
    }

    public async Task SendAsync(
        long chatId,
        ConsultationFlowResult result,
        CancellationToken cancellationToken)
    {
        if (result.Status == ConsultationFlowStatus.Question &&
            result.Step.Question is { } question)
        {
            await sender.SendTextAsync(
                chatId,
                question.Text,
                keyboards.CreateQuestionKeyboard(result.SessionId, question),
                cancellationToken);
            return;
        }

        if (result.Film is { } film)
        {
            await films.SendAsync(chatId, result.SessionId, film, cancellationToken);
            return;
        }

        await sender.SendTextAsync(
            chatId,
            TgBotTexts.NotFound,
            keyboards.CreateRetryKeyboard(),
            cancellationToken);
    }
}
