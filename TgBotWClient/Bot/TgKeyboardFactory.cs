using EsConsultationServices.Contracts;
using TL;

namespace TgBotWClient.Bot;

/// <summary>
/// Собирает reply- и inline-клавиатуры Telegram.
/// Вопрос показывается inline-кнопками (callback data), действия — reply-кнопками.
/// </summary>
public sealed class TgKeyboardFactory
{
    public const int MaxCallbackDataBytes = 64;

    private const int OptionsPerRow = 2;
    private const int MaxOptions = 16;
    private const int MaxButtonTextLength = 60;

    public ReplyKeyboardMarkup CreateStartKeyboard()
    {
        return CreateReplyKeyboard(new[]
        {
            new[] { TgBotTexts.StartConsultationButton }
        });
    }

    public ReplyKeyboardMarkup CreateQuestionKeyboard()
    {
        return CreateReplyKeyboard(new[]
        {
            new[] { TgBotTexts.StartOverButton }
        });
    }

    public ReplyKeyboardMarkup CreateRetryKeyboard()
    {
        return CreateReplyKeyboard(new[]
        {
            new[] { TgBotTexts.StartConsultationButton }
        });
    }

    public ReplyKeyboardMarkup CreateFilmKeyboard()
    {
        return CreateReplyKeyboard(new[]
        {
            new[] { TgBotTexts.AnotherButton },
            new[] { TgBotTexts.LikeButton, TgBotTexts.DislikeButton },
            new[] { TgBotTexts.StartConsultationButton }
        });
    }

    public ReplyInlineMarkup CreateQuestionKeyboard(string sessionId, ConsultationQuestion question)
    {
        var hash = ComputeQuestionHash(question);
        var options = question.Answers.Take(MaxOptions).ToList();
        var rows = new List<KeyboardInlineButtonRow>();

        for (var index = 0; index < options.Count; index += OptionsPerRow)
        {
            var buttons = options
                .Skip(index)
                .Take(OptionsPerRow)
                .Select((option, offset) => CreateAnswerButton(
                    sessionId,
                    index + offset,
                    hash,
                    string.IsNullOrWhiteSpace(option.Display) ? option.Value : option.Display))
                .ToArray();

            rows.Add(new KeyboardInlineButtonRow { buttons = buttons });
        }

        return new ReplyInlineMarkup { rows = rows.ToArray() };
    }

    /// <summary>8 hex-символов SHA-256 от имени и текста вопроса.</summary>
    public static string ComputeQuestionHash(ConsultationQuestion question)
    {
        var payload = $"{question.Name}\n{question.Text}";
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash)[..8].ToLowerInvariant();
    }

    /// <summary>callback data: answer|sessionId|index|hash (не длиннее 64 байт UTF-8).</summary>
    public static string BuildAnswerCallbackData(string sessionId, int index, string hash)
    {
        var data = $"{Contracts.TgCommandNames.Answer}|{sessionId}|{index}|{hash}";
        var size = System.Text.Encoding.UTF8.GetByteCount(data);

        if (size > MaxCallbackDataBytes)
            throw new InvalidOperationException(
                $"Callback data занимает {size} байт, допустимо не больше {MaxCallbackDataBytes}.");

        return data;
    }

    private static KeyboardInlineButton CreateAnswerButton(
        string sessionId,
        int index,
        string hash,
        string label)
    {
        var data = BuildAnswerCallbackData(sessionId, index, hash);

        return new KeyboardInlineButton
        {
            text = Shorten(label, MaxButtonTextLength),
            type = new InlineButtonTypeCallback
            {
                data = System.Text.Encoding.UTF8.GetBytes(data)
            }
        };
    }

    private static ReplyKeyboardMarkup CreateReplyKeyboard(IReadOnlyList<string[]> rows)
    {
        return new ReplyKeyboardMarkup
        {
            flags = ReplyKeyboardMarkup.Flags.resize | ReplyKeyboardMarkup.Flags.persistent,
            rows = rows
                .Select(row => new KeyboardButtonRow
                {
                    buttons = row
                        .Select(text => new KeyboardButton { text = text })
                        .ToArray()
                })
                .ToArray()
        };
    }

    private static string Shorten(string text, int maxLength)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..(maxLength - 1)] + "…";
    }
}
