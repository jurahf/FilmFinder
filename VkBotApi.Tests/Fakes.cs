using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsConsultationServices.Workflow;
using VkBotApi.Bot;
using VkBotApi.Contracts;
using VkNet.Model;

namespace VkBotApi.Tests;

internal sealed class FakeWorkflow : IConsultationWorkflow
{
    public Func<int?, CancellationToken, Task<ConsultationFlowResult>>? StartHandler { get; set; }
    public Func<string, CancellationToken, Task<ConsultationFlowResult>>? CurrentHandler { get; set; }
    public Func<string, string, string, CancellationToken, Task<ConsultationFlowResult>>? AnswerHandler { get; set; }
    public Func<string, CancellationToken, Task<ConsultationFlowResult>>? AnotherHandler { get; set; }
    public Func<string, bool, string?, CancellationToken, Task>? ReviewHandler { get; set; }

    public List<(string SessionId, bool Liked, string? Comment)> Reviews { get; } = new();

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

internal sealed class RecordingMessageSender : IVkMessageSender
{
    public List<SentText> Texts { get; } = new();
    public List<SentPhoto> Photos { get; } = new();

    public Task SendTextAsync(
        long peerId,
        string text,
        MessageKeyboard? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        Texts.Add(new SentText(peerId, text, keyboard));
        return Task.CompletedTask;
    }

    public Task SendPhotoAsync(
        long peerId,
        string photoUrl,
        CancellationToken cancellationToken = default)
    {
        Photos.Add(new SentPhoto(peerId, photoUrl));
        return Task.CompletedTask;
    }
}

internal sealed record SentText(long PeerId, string Text, MessageKeyboard? Keyboard);

internal sealed record SentPhoto(long PeerId, string Url);

internal sealed class ThrowingCommand : VkBotApi.Bot.Commands.IVkCommand
{
    public string Name => "Explode";

    public Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
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
    public static IReadOnlyList<IReadOnlyList<MessageKeyboardButton>> Rows(MessageKeyboard keyboard)
    {
        return keyboard.Buttons
            .Select(row => (IReadOnlyList<MessageKeyboardButton>)row.ToList())
            .ToList();
    }

    public static IReadOnlyList<MessageKeyboardButton> Buttons(MessageKeyboard keyboard)
    {
        return Rows(keyboard).SelectMany(x => x).ToList();
    }

    public static IReadOnlyList<string> Payloads(MessageKeyboard keyboard)
    {
        return Buttons(keyboard).Select(x => x.Action.Payload).ToList();
    }
}
