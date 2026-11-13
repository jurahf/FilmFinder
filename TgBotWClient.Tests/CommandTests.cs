using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using TgBotWClient.Bot;
using TgBotWClient.Bot.Commands;
using TgBotWClient.Contracts;
using Xunit;

namespace TgBotWClient.Tests;

public sealed class CommandTests
{
    private const long ChatId = 555;

    private readonly RecordingMessageSender sender = new();
    private readonly TgKeyboardFactory keyboards = new();
    private readonly FakeWorkflow workflow = new();
    private readonly FakeChatSessionStore sessions = new();

    private TgStepPresenter CreatePresenter()
    {
        return new TgStepPresenter(
            sender,
            keyboards,
            new TgFilmPresenter(sender, new FilmMessageFormatter(), keyboards));
    }

    private static TgIncomingMessage Message(string? text = null, string? callback = null)
    {
        return new TgIncomingMessage
        {
            ChatId = ChatId,
            Text = text,
            CallbackData = callback,
            CallbackQueryId = callback == null ? 0 : 77
        };
    }

    private static TgCommandRequest Request(string name, params string[] arguments)
    {
        return new TgCommandRequest { Name = name, Arguments = arguments };
    }

    [Fact]
    public async Task StartCommand_sends_hello_and_start_keyboard()
    {
        var command = new StartCommand(sender, keyboards);

        await command.ExecuteAsync(Message("start"), Request(command.Name), CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(ChatId, text.ChatId);
        Assert.Equal(TgBotTexts.Hello, text.Text);

        var keyboard = Assert.IsType<TL.ReplyKeyboardMarkup>(text.Keyboard);
        Assert.Equal(new[] { "Подобрать фильм" }, KeyboardReader.ReplyTexts(keyboard));
    }

    [Fact]
    public async Task ConsultationCommand_saves_binding_and_shows_first_question()
    {
        var command = new StartConsultationCommand(workflow, sessions, CreatePresenter());

        await command.ExecuteAsync(
            Message("consultation"),
            Request(command.Name),
            CancellationToken.None);

        Assert.Equal("session-1", sessions.Bindings[ChatId]);
        Assert.Equal(2, sender.Texts.Count);
        Assert.Equal(TgBotTexts.Started, sender.Texts[0].Text);

        var reply = Assert.IsType<TL.ReplyKeyboardMarkup>(sender.Texts[0].Keyboard);
        Assert.Equal(new[] { "Начать заново" }, KeyboardReader.ReplyTexts(reply));

        Assert.Equal("Какое настроение?", sender.Texts[1].Text);
        var inline = Assert.IsType<TL.ReplyInlineMarkup>(sender.Texts[1].Keyboard);
        Assert.Equal(new[] { "Хорошее", "Плохое" }, KeyboardReader.InlineTexts(inline));
    }

    [Fact]
    public async Task AnswerCommand_answers_with_current_question_and_shows_film()
    {
        var question = FlowFactory.Question().Step.Question!;
        var hash = TgKeyboardFactory.ComputeQuestionHash(question);
        workflow.AnswerHandler = (sessionId, _, _, _) =>
            Task.FromResult(FlowFactory.Film(sessionId));

        var command = new SetAnswerAndNextCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter());

        await command.ExecuteAsync(
            Message(callback: $"answer|session-1|0|{hash}"),
            Request(command.Name, "session-1", "0", hash),
            CancellationToken.None);

        var answer = Assert.Single(workflow.Answers);
        Assert.Equal("session-1", answer.SessionId);
        Assert.Equal("Mood", answer.Variable);
        Assert.Equal("Good", answer.Value);

        Assert.Equal(TgBotTexts.SearchingFilm, sender.Texts[0].Text);
        Assert.Single(sender.Photos);
        Assert.Equal("https://example.com/poster.jpg", sender.Photos[0].Url);

        var card = sender.Texts[1];
        Assert.Contains("Фильм (2001)", card.Text);
        var keyboard = Assert.IsType<TL.ReplyKeyboardMarkup>(card.Keyboard);
        Assert.Contains("Другой фильм", KeyboardReader.ReplyTexts(keyboard));
    }

    [Fact]
    public async Task AnswerCommand_rejects_stale_question_without_answering()
    {
        var command = new SetAnswerAndNextCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter());

        await command.ExecuteAsync(
            Message(callback: "answer|session-1|0|deadbeef"),
            Request(command.Name, "session-1", "0", "deadbeef"),
            CancellationToken.None);

        Assert.Empty(workflow.Answers);
        Assert.False(string.IsNullOrWhiteSpace(TgBotTexts.StaleQuestion));
        Assert.Equal(TgBotTexts.StaleQuestion, sender.Texts[0].Text);
        Assert.Equal("Какое настроение?", sender.Texts[1].Text);
    }

    [Fact]
    public async Task AnswerCommand_shows_not_found_without_film()
    {
        var question = FlowFactory.Question().Step.Question!;
        var hash = TgKeyboardFactory.ComputeQuestionHash(question);
        workflow.AnswerHandler = (sessionId, _, _, _) =>
            Task.FromResult(FlowFactory.NotFound(sessionId));

        var command = new SetAnswerAndNextCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter());

        await command.ExecuteAsync(
            Message(callback: $"answer|session-1|0|{hash}"),
            Request(command.Name, "session-1", "0", hash),
            CancellationToken.None);

        Assert.Equal(2, sender.Texts.Count);
        Assert.Equal(TgBotTexts.NotFound, sender.Texts[1].Text);
        Assert.Empty(sender.Photos);

        var keyboard = Assert.IsType<TL.ReplyKeyboardMarkup>(sender.Texts[1].Keyboard);
        Assert.Equal(new[] { "Подобрать фильм" }, KeyboardReader.ReplyTexts(keyboard));
    }

    [Fact]
    public async Task AnotherCommand_uses_binding_and_shows_next_film()
    {
        sessions.Bindings[ChatId] = "session-1";
        string? requestedSession = null;
        workflow.AnotherHandler = (sessionId, _) =>
        {
            requestedSession = sessionId;
            return Task.FromResult(FlowFactory.Film(sessionId, filmId: 43, name: "Другой фильм"));
        };

        var command = new AnotherCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter(),
            keyboards);

        await command.ExecuteAsync(
            Message("Другой фильм"),
            Request(command.Name),
            CancellationToken.None);

        Assert.Equal("session-1", requestedSession);
        Assert.Equal(TgBotTexts.SearchingAnother, sender.Texts[0].Text);
        Assert.Contains("Другой фильм (2001)", sender.Texts[1].Text);
    }

    [Fact]
    public async Task AnotherCommand_without_binding_asks_to_start_consultation()
    {
        var command = new AnotherCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter(),
            keyboards);

        await command.ExecuteAsync(
            Message("Другой фильм"),
            Request(command.Name),
            CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(TgBotTexts.NoBinding, text.Text);

        var keyboard = Assert.IsType<TL.ReplyKeyboardMarkup>(text.Keyboard);
        Assert.Equal(new[] { "Подобрать фильм" }, KeyboardReader.ReplyTexts(keyboard));
    }

    [Fact]
    public async Task AnotherCommand_clears_binding_when_no_more_films()
    {
        sessions.Bindings[ChatId] = "session-1";
        workflow.AnotherHandler = (_, _) => throw new KeyNotFoundException();

        var command = new AnotherCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter(),
            keyboards);

        await command.ExecuteAsync(
            Message("Другой фильм"),
            Request(command.Name),
            CancellationToken.None);

        Assert.False(sessions.Bindings.ContainsKey(ChatId));
        Assert.Equal(TgBotTexts.NoMoreFilms, sender.Texts[1].Text);
    }

    [Fact]
    public async Task AnotherCommand_reports_unfinished_consultation()
    {
        sessions.Bindings[ChatId] = "session-1";
        workflow.AnotherHandler = (_, _) =>
            throw new InvalidOperationException("not completed");

        var command = new AnotherCommand(
            workflow,
            sessions,
            sender,
            CreatePresenter(),
            keyboards);

        await command.ExecuteAsync(
            Message("Другой фильм"),
            Request(command.Name),
            CancellationToken.None);

        Assert.Equal(TgBotTexts.NotCompleted, sender.Texts[1].Text);
        Assert.True(sessions.Bindings.ContainsKey(ChatId));
    }

    [Theory]
    [InlineData(TgEstimation.Positive, true)]
    [InlineData("positive", true)]
    [InlineData(TgEstimation.Negative, false)]
    public async Task ReviewCommand_maps_positive_and_negative(string estimation, bool expectedLiked)
    {
        sessions.Bindings[ChatId] = "session-1";

        var command = new ReviewCommand(
            workflow,
            sessions,
            sender,
            keyboards,
            NullLogger<ReviewCommand>.Instance);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, estimation),
            CancellationToken.None);

        var review = Assert.Single(workflow.Reviews);
        Assert.Equal("session-1", review.SessionId);
        Assert.Equal(expectedLiked, review.Liked);
        Assert.Matches(
            new Regex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2} - (Positive|Negative)$"),
            review.Comment);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(TgBotTexts.ThanksForReview, text.Text);
    }

    [Fact]
    public async Task ReviewCommand_without_binding_does_not_write_review()
    {
        var command = new ReviewCommand(
            workflow,
            sessions,
            sender,
            keyboards,
            NullLogger<ReviewCommand>.Instance);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, TgEstimation.Positive),
            CancellationToken.None);

        Assert.Empty(workflow.Reviews);
        Assert.Equal(TgBotTexts.NoBinding, Assert.Single(sender.Texts).Text);
    }

    [Fact]
    public async Task ReviewCommand_still_thanks_when_session_is_missing()
    {
        sessions.Bindings[ChatId] = "missing";
        workflow.ReviewHandler = (_, _, _, _) => throw new KeyNotFoundException();

        var command = new ReviewCommand(
            workflow,
            sessions,
            sender,
            keyboards,
            NullLogger<ReviewCommand>.Instance);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, TgEstimation.Positive),
            CancellationToken.None);

        Assert.Equal(TgBotTexts.ThanksForReview, Assert.Single(sender.Texts).Text);
        Assert.False(sessions.Bindings.ContainsKey(ChatId));
    }
}

public sealed class TgBotHandlerTests
{
    private readonly RecordingMessageSender sender = new();
    private readonly TgKeyboardFactory keyboards = new();

    [Fact]
    public async Task Handler_sends_error_and_acknowledges_callback_on_exception()
    {
        var handler = new TgBotHandler(
            new[] { new ThrowingCommand() },
            new TgBotWClient.Parsing.TgCommandParser(),
            sender,
            keyboards,
            NullLogger<TgBotHandler>.Instance);

        await handler.HandleAsync(
            new TgIncomingMessage
            {
                ChatId = 9,
                CallbackData = "Explode|1",
                CallbackQueryId = 77
            },
            CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(9, text.ChatId);
        Assert.Equal(TgBotTexts.Error, text.Text);
        Assert.Equal(new long[] { 77 }, sender.AcknowledgedCallbacks);
    }

    [Fact]
    public async Task Handler_sends_hint_for_unknown_text()
    {
        var handler = new TgBotHandler(
            new[] { new ThrowingCommand() },
            new TgBotWClient.Parsing.TgCommandParser(),
            sender,
            keyboards,
            NullLogger<TgBotHandler>.Instance);

        await handler.HandleAsync(
            new TgIncomingMessage { ChatId = 9, Text = "Привет" },
            CancellationToken.None);

        Assert.Equal(TgBotTexts.UnknownCommand, Assert.Single(sender.Texts).Text);
    }

    [Fact]
    public async Task Handler_acknowledges_unknown_callback_without_message()
    {
        var handler = new TgBotHandler(
            new[] { new ThrowingCommand() },
            new TgBotWClient.Parsing.TgCommandParser(),
            sender,
            keyboards,
            NullLogger<TgBotHandler>.Instance);

        await handler.HandleAsync(
            new TgIncomingMessage
            {
                ChatId = 9,
                CallbackData = "unknown|1",
                CallbackQueryId = 88
            },
            CancellationToken.None);

        Assert.Empty(sender.Texts);
        Assert.Equal(new long[] { 88 }, sender.AcknowledgedCallbacks);
    }
}
