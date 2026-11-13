using EsConsultationServices.Contracts;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace VkBotApi.Bot;

/// <summary>
/// Собирает клавиатуры с прежними payload бота.
/// </summary>
public sealed class VkKeyboardFactory
{
    private const int OptionsPerRow = 3;
    private const int MaxOptionRows = 9;

    public MessageKeyboard CreateConsultationKeyboard()
    {
        return CreateSingleButton(
            "Подобрать!",
            "Consultation",
            KeyboardButtonColor.Positive);
    }

    public MessageKeyboard CreateRetryKeyboard()
    {
        return CreateSingleButton(
            VkBotTexts.TryAgain,
            "Consultation",
            KeyboardButtonColor.Positive);
    }

    public MessageKeyboard CreateQuestionKeyboard(string sessionId, ConsultationQuestion question)
    {
        var builder = new KeyboardBuilder();
        var options = question.Answers
            .Take(MaxOptionRows * OptionsPerRow)
            .ToList();

        for (var index = 0; index < options.Count; index++)
        {
            if (index > 0 && index % OptionsPerRow == 0)
                builder.AddLine();

            var option = options[index];
            var label = string.IsNullOrWhiteSpace(option.Display) ? option.Value : option.Display;
            var payload = $"SetAnswerAndNext|{sessionId}|{question.Name}|{option.Value}";

            builder.AddButton(CreateAction(label, payload), KeyboardButtonColor.Secondary);
        }

        if (options.Count > 0)
            builder.AddLine();

        builder.AddButton(
            CreateAction(VkBotTexts.StartOver, "Consultation"),
            KeyboardButtonColor.Negative);

        return builder.Build();
    }

    public MessageKeyboard CreateFilmKeyboard(string sessionId, int filmId)
    {
        var builder = new KeyboardBuilder();

        builder.AddButton(
            CreateAction("Подобрать еще один фильм!", "Consultation"),
            KeyboardButtonColor.Positive);

        builder.AddLine();
        builder.AddButton(
            CreateAction("Другой по тем же параметрам", $"Another|{sessionId}|{filmId}"),
            KeyboardButtonColor.Secondary);

        builder.AddLine();
        builder.AddButton(
            CreateAction("👎🏻", $"Review|{sessionId}|Negative"),
            KeyboardButtonColor.Secondary);
        builder.AddButton(
            CreateAction("👍🏻", $"Review|{sessionId}|Positive"),
            KeyboardButtonColor.Secondary);

        return builder.Build();
    }

    private static MessageKeyboard CreateSingleButton(
        string label,
        string payload,
        KeyboardButtonColor color)
    {
        var builder = new KeyboardBuilder();
        builder.AddButton(CreateAction(label, payload), color);
        return builder.Build();
    }

    private static MessageKeyboardButtonAction CreateAction(string label, string payload)
    {
        return new MessageKeyboardButtonAction
        {
            Type = KeyboardButtonActionType.Text,
            Label = label,
            Payload = payload
        };
    }
}
