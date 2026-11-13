using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsConsultationServices.Workflow;
using TgBotWClient.Bot;
using TgBotWClient.Contracts;
using TL;

namespace TgBotWClient.Tests;

internal sealed class FakeWorkflow : IConsultationWorkflow
{
    public Func<int?, CancellationToken, Task<ConsultationFlowResult>>? StartHandler { get; set; }
    public Func<string, CancellationToken, Task<ConsultationFlowResult>>? CurrentHandler { get; set; }
    public Func<string, string, string, CancellationToken, Task<ConsultationFlowResult>>? AnswerHandler { get; set; }
    public Func<string, CancellationToken, Task<ConsultationFlowResult>>? AnotherHandler { get; set; }
    public Func<string, bool, string?, CancellationToken, Task>? ReviewHandler { get; set; }

    public List<(string SessionId, bool Liked, string? Comment)> Reviews { get; } = new();
    public List<(string SessionId, string Variable, string Value)> Answers { get; } = new();

    public Task<ConsultationFlowResult> StartAsync(
        int? expertSystemId,
        CancellationToken cancellationToken = default)
    {
        return StartHandler?.Invoke(expertSystemId, cancellationToken)
            ?? Task.FromResult(FlowFactory.Question());
    }

    public Task<ConsultationFlowResult> GetCurrentAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return CurrentHandler?.Invoke(sessionId, cancellationToken)
            ?? Task.FromResult(FlowFactory.Question(sessionId));
    }

    public Task<ConsultationFlowResult> AnswerAsync(
        string sessionId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default)
    {
        Answers.Add((sessionId, variableName, value));

        return AnswerHandler?.Invoke(sessionId, variableName, value, cancellationToken)
            ?? Task.FromResult(FlowFactory.Question(sessionId));
    }

    public Task<ConsultationFlowResult> GetAnotherAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return AnotherHandler?.Invoke(sessionId, cancellationToken)
            ?? Task.FromResult(FlowFactory.Film(sessionId));
    }

    public Task ReviewAsync(
        string sessionId,
        bool liked,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        Reviews.Add((sessionId, liked, comment));
        return ReviewHandler?.Invoke(sessionId, liked, comment, cancellationToken)
            ?? Task.CompletedTask;
    }
}

internal sealed class FakeChatSessionStore : ITgChatSessionStore
{
    public Dictionary<long, string> Bindings { get; } = new();

    public Task<string?> GetSessionIdAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Bindings.TryGetValue(chatId, out var sessionId)
            ? sessionId
            : null);
    }

    public Task SetSessionIdAsync(
        long chatId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        Bindings[chatId] = sessionId;
        return Task.CompletedTask;
    }

    public Task ClearAsync(long chatId, CancellationToken cancellationToken = default)
    {
        Bindings.Remove(chatId);
        return Task.CompletedTask;
    }
}

internal sealed class RecordingMessageSender : ITgMessageSender
{
    public List<SentText> Texts { get; } = new();
    public List<SentPhoto> Photos { get; } = new();
    public List<long> AcknowledgedCallbacks { get; } = new();

    public Task SendTextAsync(
        long chatId,
        string text,
        ReplyMarkup? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        Texts.Add(new SentText(chatId, text, keyboard));
        return Task.CompletedTask;
    }

    public Task SendPhotoAsync(
        long chatId,
        string photoUrl,
        ReplyMarkup? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        Photos.Add(new SentPhoto(chatId, photoUrl));
        return Task.CompletedTask;
    }

    public Task AnswerCallbackAsync(
        long callbackQueryId,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        AcknowledgedCallbacks.Add(callbackQueryId);
        return Task.CompletedTask;
    }
}

internal sealed record SentText(long ChatId, string Text, ReplyMarkup? Keyboard);

internal sealed record SentPhoto(long ChatId, string Url);

internal sealed class ThrowingCommand : TgBotWClient.Bot.Commands.ITgCommand
{
    public string Name => "Explode";

    public Task ExecuteAsync(
        TgIncomingMessage message,
        TgCommandRequest request,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("boom");
    }
}

internal static class FlowFactory
{
    public static ConsultationFlowResult Question(
        string sessionId = "session-1",
        string name = "Mood",
        string text = "Какое настроение?",
        params (string Display, string Value)[] options)
    {
        if (options.Length == 0)
            options = new[] { ("Хорошее", "Good"), ("Плохое", "Bad") };

        return new ConsultationFlowResult
        {
            SessionId = sessionId,
            ExpertSystemId = 7,
            ExpertSystemName = "Эксперт",
            Status = ConsultationFlowStatus.Question,
            Step = new ConsultationStep
            {
                Question = new ConsultationQuestion
                {
                    Name = name,
                    Text = text,
                    Answers = options
                        .Select(x => new ConsultationAnswerOption
                        {
                            Display = x.Display,
                            Value = x.Value
                        })
                        .ToList()
                }
            }
        };
    }

    public static ConsultationFlowResult Film(
        string sessionId = "session-1",
        int filmId = 42,
        string name = "Фильм",
        string? poster = "https://example.com/poster.jpg")
    {
        return new ConsultationFlowResult
        {
            SessionId = sessionId,
            ExpertSystemId = 7,
            ExpertSystemName = "Эксперт",
            Status = ConsultationFlowStatus.Completed,
            Film = new RecommendedFilm
            {
                Id = filmId,
                Name = name,
                Year = 2001,
                Rating = 8.1m,
                Description = "Описание фильма",
                Poster = poster
            }
        };
    }

    public static ConsultationFlowResult NotFound(string sessionId = "session-1")
    {
        return new ConsultationFlowResult
        {
            SessionId = sessionId,
            ExpertSystemId = 7,
            ExpertSystemName = "Эксперт",
            Status = ConsultationFlowStatus.Unknown
        };
    }
}

internal static class KeyboardReader
{
    public static IReadOnlyList<IReadOnlyList<string>> ReplyRows(ReplyKeyboardMarkup keyboard)
    {
        return keyboard.rows
            .Select(row => (IReadOnlyList<string>)row.buttons.Select(x => x.text).ToList())
            .ToList();
    }

    public static IReadOnlyList<string> ReplyTexts(ReplyKeyboardMarkup keyboard)
    {
        return ReplyRows(keyboard).SelectMany(x => x).ToList();
    }

    public static IReadOnlyList<string> InlineTexts(ReplyInlineMarkup keyboard)
    {
        return keyboard.rows.SelectMany(x => x.buttons).Select(x => x.text).ToList();
    }

    public static IReadOnlyList<string> InlineCallbackData(ReplyInlineMarkup keyboard)
    {
        return keyboard.rows
            .SelectMany(x => x.buttons)
            .Select(x => System.Text.Encoding.UTF8.GetString(
                ((InlineButtonTypeCallback)x.type).data))
            .ToList();
    }
}
