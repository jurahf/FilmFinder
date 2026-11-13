using EsConsultationServices.Films;
using TgBotWClient.Bot;
using Xunit;

namespace TgBotWClient.Tests;

public sealed class KeyboardAndFormatterTests
{
    private readonly TgKeyboardFactory keyboards = new();

    [Fact]
    public void Start_keyboard_has_single_consultation_button()
    {
        var keyboard = keyboards.CreateStartKeyboard();

        Assert.Equal(new[] { new[] { "Подобрать фильм" } }, KeyboardReader.ReplyRows(keyboard));
    }

    [Fact]
    public void Question_reply_keyboard_allows_starting_over()
    {
        var keyboard = keyboards.CreateQuestionKeyboard();

        Assert.Equal(new[] { new[] { "Начать заново" } }, KeyboardReader.ReplyRows(keyboard));
    }

    [Fact]
    public void Film_keyboard_has_actions_in_expected_rows()
    {
        var keyboard = keyboards.CreateFilmKeyboard();

        Assert.Equal(
            new[]
            {
                new[] { "Другой фильм" },
                new[] { "👍 Ок", "👎 Не ок" },
                new[] { "Подобрать фильм" }
            },
            KeyboardReader.ReplyRows(keyboard));
    }

    [Fact]
    public void Question_keyboard_uses_two_buttons_per_row()
    {
        var question = FlowFactory.Question(
            options: new[]
            {
                ("Хорошее", "Good"),
                ("Плохое", "Bad"),
                ("Нейтральное", "Neutral")
            }).Step.Question!;

        var keyboard = keyboards.CreateQuestionKeyboard("session-1", question);

        Assert.Equal(new[] { "Хорошее", "Плохое", "Нейтральное" }, KeyboardReader.InlineTexts(keyboard));
        Assert.Equal(2, keyboard.rows.Length);
        Assert.Equal(2, keyboard.rows[0].buttons.Length);
        Assert.Single(keyboard.rows[1].buttons);
    }

    [Fact]
    public void Answer_callback_data_contains_index_and_hash_within_64_bytes()
    {
        var question = FlowFactory.Question().Step.Question!;
        var sessionId = new string('a', 32);

        var keyboard = keyboards.CreateQuestionKeyboard(sessionId, question);
        var callbacks = KeyboardReader.InlineCallbackData(keyboard);
        var hash = TgKeyboardFactory.ComputeQuestionHash(question);

        Assert.Equal(
            $"answer|{sessionId}|0|{hash}",
            callbacks[0]);
        Assert.Equal($"answer|{sessionId}|1|{hash}", callbacks[1]);
        Assert.All(
            callbacks,
            x => Assert.True(
                System.Text.Encoding.UTF8.GetByteCount(x) <= TgKeyboardFactory.MaxCallbackDataBytes));
        Assert.Equal(8, hash.Length);
    }

    [Fact]
    public void Question_hash_depends_on_name_and_text()
    {
        var first = FlowFactory.Question(text: "Первый?").Step.Question!;
        var second = FlowFactory.Question(text: "Второй?").Step.Question!;

        Assert.NotEqual(
            TgKeyboardFactory.ComputeQuestionHash(first),
            TgKeyboardFactory.ComputeQuestionHash(second));
    }

    [Fact]
    public void Callback_data_over_limit_is_rejected()
    {
        var sessionId = new string('a', 60);

        Assert.Throws<InvalidOperationException>(
            () => TgKeyboardFactory.BuildAnswerCallbackData(sessionId, 1, "deadbeef"));
    }

    [Fact]
    public void Film_card_contains_main_fields()
    {
        var film = new RecommendedFilm
        {
            Id = 1,
            Name = "Матрица",
            Year = 1999,
            Rating = 8.7m,
            Description = "Описание",
            Tags = new[]
            {
                new FilmTag { Name = "интересный сюжет", Percent = 80 }
            }
        };

        var text = new FilmMessageFormatter().Format(film);

        Assert.Contains("Матрица (1999)", text);
        Assert.Contains("Описание", text);
        Assert.Contains("8,70", text);
        Assert.Contains("80% интересный сюжет", text);
    }
}
