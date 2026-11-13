using TgBotWClient.Contracts;
using TgBotWClient.Parsing;
using Xunit;

namespace TgBotWClient.Tests;

public sealed class ParsingTests
{
    private readonly TgCommandParser parser = new();

    [Fact]
    public void Callback_answer_is_parsed_with_arguments()
    {
        var request = parser.Parse(new TgIncomingMessage
        {
            ChatId = 1,
            CallbackData = "answer|SessionABC|2|HashA1"
        });

        Assert.NotNull(request);
        Assert.Equal(TgCommandNames.Answer, request!.Name);
        Assert.Equal(new[] { "SessionABC", "2", "HashA1" }, request.Arguments);
    }

    [Theory]
    [InlineData("/start", TgCommandNames.Start)]
    [InlineData("/start@FilmFinderBot", TgCommandNames.Start)]
    [InlineData("/help", TgCommandNames.Start)]
    [InlineData("/START", TgCommandNames.Start)]
    [InlineData("/consultation", TgCommandNames.Consultation)]
    [InlineData("/new", TgCommandNames.Consultation)]
    [InlineData("/another", TgCommandNames.Another)]
    public void Slash_commands_are_recognized(string text, string expectedName)
    {
        var request = parser.Parse(new TgIncomingMessage { ChatId = 1, Text = text });

        Assert.NotNull(request);
        Assert.Equal(expectedName, request!.Name);
    }

    [Theory]
    [InlineData("Подобрать фильм", TgCommandNames.Consultation)]
    [InlineData("Начать заново", TgCommandNames.Consultation)]
    [InlineData("Другой фильм", TgCommandNames.Another)]
    public void Reply_buttons_are_recognized(string text, string expectedName)
    {
        var request = parser.Parse(new TgIncomingMessage { ChatId = 1, Text = text });

        Assert.NotNull(request);
        Assert.Equal(expectedName, request!.Name);
    }

    [Theory]
    [InlineData("👍 Ок", TgEstimation.Positive)]
    [InlineData("Ок", TgEstimation.Positive)]
    [InlineData("/ok", TgEstimation.Positive)]
    [InlineData("👎 Не ок", TgEstimation.Negative)]
    [InlineData("Не ок", TgEstimation.Negative)]
    [InlineData("/bad", TgEstimation.Negative)]
    public void Review_buttons_and_commands_are_recognized(string text, string expectedEstimation)
    {
        var request = parser.Parse(new TgIncomingMessage { ChatId = 1, Text = text });

        Assert.NotNull(request);
        Assert.Equal(TgCommandNames.Review, request!.Name);
        Assert.Equal(expectedEstimation, request.GetArgument(0));
    }

    [Theory]
    [InlineData("Привет")]
    [InlineData("/unknown")]
    [InlineData("")]
    public void Unknown_text_is_ignored(string text)
    {
        var request = parser.Parse(new TgIncomingMessage { ChatId = 1, Text = text });

        Assert.Null(request);
    }
}
