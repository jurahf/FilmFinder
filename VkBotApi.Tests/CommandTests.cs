using System.Text.RegularExpressions;
using EsConsultationServices.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using VkBotApi.Bot;
using VkBotApi.Bot.Commands;
using VkBotApi.Contracts;
using Xunit;

namespace VkBotApi.Tests;

public sealed class CommandTests
{
    private readonly RecordingMessageSender sender = new();
    private readonly VkKeyboardFactory keyboards = new();
    private readonly FakeWorkflow workflow = new();

    private VkStepPresenter CreatePresenter()
    {
        return new VkStepPresenter(
            sender,
            keyboards,
            new VkFilmPresenter(sender, new FilmMessageFormatter(), keyboards));
    }

    private static VkIncomingMessage Message(params string[] arguments)
    {
        return new VkIncomingMessage { PeerId = 555, FromId = 555, Text = arguments.FirstOrDefault() };
    }

    private static VkCommandRequest Request(string name, params string[] arguments)
    {
        return new VkCommandRequest { Name = name, Arguments = arguments };
    }

    [Fact]
    public async Task StartCommand_sends_hello_and_consultation_keyboard()
    {
        var command = new StartCommand(sender, keyboards);

        await command.ExecuteAsync(Message("start"), Request(command.Name), CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(555, text.PeerId);
        Assert.Equal(VkBotTexts.Hello, text.Text);
        Assert.NotNull(text.Keyboard);
        Assert.Equal(new[] { "Consultation" }, KeyboardReader.Payloads(text.Keyboard!));
    }

    [Fact]
    public async Task ConsultationCommand_sends_first_question()
    {
        var command = new StartConsultationCommand(workflow, CreatePresenter());

        await command.ExecuteAsync(Message("Consultation"), Request(command.Name), CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal("Какое настроение?", text.Text);
        Assert.NotNull(text.Keyboard);
        Assert.Equal(
            new[]
            {
                "SetAnswerAndNext|session-1|Mood|Good",
                "SetAnswerAndNext|session-1|Mood|Bad",
                "Consultation"
            },
            KeyboardReader.Payloads(text.Keyboard!));
    }

    [Fact]
    public async Task AnswerCommand_sends_film_card_and_keyboard()
    {
        string? answerSessionId = null;
        string? answerVariable = null;
        string? answerValue = null;

        workflow.AnswerHandler = (sessionId, variable, value, _) =>
        {
            answerSessionId = sessionId;
            answerVariable = variable;
            answerValue = value;
            return Task.FromResult(FlowFactory.Film(sessionId));
        };
        var command = new SetAnswerAndNextCommand(workflow, sender, CreatePresenter());

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, "session-1", "Mood", "Good"),
            CancellationToken.None);

        Assert.Equal("session-1", answerSessionId);
        Assert.Equal("Mood", answerVariable);
        Assert.Equal("Good", answerValue);

        Assert.Equal(VkBotTexts.SearchingFilm, sender.Texts[0].Text);
        Assert.Single(sender.Photos);
        Assert.Equal("https://example.com/poster.jpg", sender.Photos[0].Url);

        var card = sender.Texts[1];
        Assert.Contains("Фильм (2001)", card.Text);
        Assert.NotNull(card.Keyboard);
        Assert.Contains("Another|session-1|42", KeyboardReader.Payloads(card.Keyboard!));
    }

    [Fact]
    public async Task AnswerCommand_sends_not_found_without_film()
    {
        workflow.AnswerHandler = (sessionId, _, _, _) =>
            Task.FromResult(FlowFactory.NotFound(sessionId));
        var command = new SetAnswerAndNextCommand(workflow, sender, CreatePresenter());

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, "session-1", "Mood", "Good"),
            CancellationToken.None);

        Assert.Equal(2, sender.Texts.Count);
        Assert.Equal(VkBotTexts.SearchingFilm, sender.Texts[0].Text);
        Assert.Equal(VkBotTexts.NotFound, sender.Texts[1].Text);
        Assert.Empty(sender.Photos);
        Assert.Equal(
            new[] { "Consultation" },
            KeyboardReader.Payloads(sender.Texts[1].Keyboard!));
    }

    [Fact]
    public async Task AnotherCommand_sends_next_film()
    {
        workflow.AnotherHandler = (sessionId, _) =>
            Task.FromResult(FlowFactory.Film(sessionId, filmId: 43, name: "Другой фильм"));
        var command = new AnotherCommand(workflow, sender, CreatePresenter(), keyboards);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, "session-1", "42"),
            CancellationToken.None);

        Assert.Equal(VkBotTexts.SearchingAnother, sender.Texts[0].Text);
        Assert.Contains("Другой фильм (2001)", sender.Texts[1].Text);
        Assert.Contains("Another|session-1|43", KeyboardReader.Payloads(sender.Texts[1].Keyboard!));
    }

    [Fact]
    public async Task AnotherCommand_reports_not_found_for_expired_session()
    {
        workflow.AnotherHandler = (_, _) => throw new KeyNotFoundException();
        var command = new AnotherCommand(workflow, sender, CreatePresenter(), keyboards);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, "missing", "42"),
            CancellationToken.None);

        Assert.Equal(2, sender.Texts.Count);
        Assert.Equal(VkBotTexts.NotFound, sender.Texts[1].Text);
        Assert.Equal(
            new[] { "Consultation" },
            KeyboardReader.Payloads(sender.Texts[1].Keyboard!));
    }

    [Theory]
    [InlineData("Positive", true)]
    [InlineData("positive", true)]
    [InlineData("Negative", false)]
    public async Task ReviewCommand_maps_positive_and_negative(string estimation, bool expectedLiked)
    {
        var command = new ReviewCommand(
            workflow,
            sender,
            keyboards,
            NullLogger<ReviewCommand>.Instance);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, "session-1", estimation),
            CancellationToken.None);

        var review = Assert.Single(workflow.Reviews);
        Assert.Equal("session-1", review.SessionId);
        Assert.Equal(expectedLiked, review.Liked);
        Assert.Matches(
            new Regex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2} - (Positive|Negative)$"),
            review.Comment);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(VkBotTexts.ThanksForReview, text.Text);
        Assert.Equal(
            new[] { "Consultation" },
            KeyboardReader.Payloads(text.Keyboard!));
    }

    [Fact]
    public async Task ReviewCommand_still_thanks_when_session_is_missing()
    {
        workflow.ReviewHandler = (_, _, _, _) => throw new KeyNotFoundException();
        var command = new ReviewCommand(
            workflow,
            sender,
            keyboards,
            NullLogger<ReviewCommand>.Instance);

        await command.ExecuteAsync(
            Message(),
            Request(command.Name, "missing", "Positive"),
            CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(VkBotTexts.ThanksForReview, text.Text);
    }
}

public sealed class VkBotHandlerTests
{
    private readonly RecordingMessageSender sender = new();
    private readonly VkKeyboardFactory keyboards = new();

    [Fact]
    public async Task Handler_sends_error_on_exception()
    {
        var handler = new VkBotHandler(
            new[] { new ThrowingCommand() },
            new VkBotApi.Parsing.VkCommandParser(),
            sender,
            keyboards,
            NullLogger<VkBotHandler>.Instance);

        await handler.HandleAsync(
            new VkIncomingMessage { PeerId = 9, Text = "Explode" },
            CancellationToken.None);

        var text = Assert.Single(sender.Texts);
        Assert.Equal(9, text.PeerId);
        Assert.Equal(VkBotTexts.Error, text.Text);
        Assert.Equal(
            new[] { "Consultation" },
            KeyboardReader.Payloads(text.Keyboard!));
    }

    [Fact]
    public async Task Handler_ignores_unknown_command()
    {
        var handler = new VkBotHandler(
            new[] { new ThrowingCommand() },
            new VkBotApi.Parsing.VkCommandParser(),
            sender,
            keyboards,
            NullLogger<VkBotHandler>.Instance);

        await handler.HandleAsync(
            new VkIncomingMessage { PeerId = 9, Text = "Unknown" },
            CancellationToken.None);

        Assert.Empty(sender.Texts);
        Assert.Empty(sender.Photos);
    }
}
