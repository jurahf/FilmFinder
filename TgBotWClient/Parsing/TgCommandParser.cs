using TgBotWClient.Bot;
using TgBotWClient.Contracts;

namespace TgBotWClient.Parsing;

/// <summary>
/// Превращает обновление Telegram в команду.
/// Callback data имеет приоритет над текстом; аргументы сохраняются как есть.
/// </summary>
public sealed class TgCommandParser
{
    private static readonly Dictionary<string, string> ButtonCommands =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [TgBotTexts.StartConsultationButton] = TgCommandNames.Consultation,
            [TgBotTexts.StartOverButton] = TgCommandNames.Consultation,
            [TgBotTexts.AnotherButton] = TgCommandNames.Another,
            [TgBotTexts.LikeButton] = TgCommandNames.Review,
            ["Ок"] = TgCommandNames.Review,
            ["Ок 👍"] = TgCommandNames.Review,
            [TgBotTexts.DislikeButton] = TgCommandNames.Review,
            ["Не ок"] = TgCommandNames.Review,
            ["Не ок 👎"] = TgCommandNames.Review
        };

    public TgCommandRequest? Parse(TgIncomingMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.CallbackData))
            return ParseCallback(message.CallbackData);

        var text = message.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.StartsWith('/')
            ? ParseSlashCommand(text)
            : ParseButtonText(text);
    }

    private static TgCommandRequest? ParseCallback(string callbackData)
    {
        var parts = callbackData.Split('|');
        var name = parts[0].Trim();
        if (name.Length == 0)
            return null;

        return new TgCommandRequest
        {
            Name = name,
            Arguments = parts.Skip(1).ToArray()
        };
    }

    private static TgCommandRequest? ParseSlashCommand(string text)
    {
        var body = text[1..];
        var separator = body.IndexOfAny(new[] { ' ', '\t' });
        var name = (separator >= 0 ? body[..separator] : body).ToLowerInvariant();

        // /start@MyBot тоже должен работать.
        var at = name.IndexOf('@');
        if (at >= 0)
            name = name[..at];

        return name switch
        {
            "start" or "help" => new TgCommandRequest { Name = TgCommandNames.Start },
            "consultation" or "begin" or "new" => new TgCommandRequest { Name = TgCommandNames.Consultation },
            "another" or "more" => new TgCommandRequest { Name = TgCommandNames.Another },
            "ok" or "like" => CreateReviewRequest(TgEstimation.Positive),
            "bad" or "dislike" => CreateReviewRequest(TgEstimation.Negative),
            _ => null
        };
    }

    private static TgCommandRequest? ParseButtonText(string text)
    {
        if (!ButtonCommands.TryGetValue(text, out var command))
            return null;

        return command == TgCommandNames.Review
            ? CreateReviewRequest(
                string.Equals(text, TgBotTexts.LikeButton, StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("Ок", StringComparison.OrdinalIgnoreCase)
                    ? TgEstimation.Positive
                    : TgEstimation.Negative)
            : new TgCommandRequest { Name = command };
    }

    private static TgCommandRequest CreateReviewRequest(string estimation)
    {
        return new TgCommandRequest
        {
            Name = TgCommandNames.Review,
            Arguments = new[] { estimation }
        };
    }
}
