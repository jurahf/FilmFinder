using EsConsultationServices.Films;
using VkBotApi.Bot;
using Xunit;

namespace VkBotApi.Tests;

public sealed class KeyboardFactoryTests
{
    private readonly VkKeyboardFactory factory = new();

    [Fact]
    public void QuestionKeyboard_has_three_buttons_per_row_and_reset()
    {
        var question = FlowFactory.Question(
            sessionId: "sid",
            options: new[]
            {
                ("Один", "One"),
                ("Два", "Two"),
                ("Три", "Three"),
                ("Четыре", "Four"),
                ("Пять", "Five"),
                ("Шесть", "Six"),
                ("Семь", "Seven"),
                ("Восемь", "Eight")
            }).Step.Question!;

        var keyboard = factory.CreateQuestionKeyboard("sid", question);
        var rows = KeyboardReader.Rows(keyboard);

        Assert.Equal(new[] { 3, 3, 2, 1 }, rows.Select(x => x.Count));
        Assert.All(
            rows.Take(3).SelectMany(x => x),
            button => Assert.Equal("Secondary", button.Color.ToString()));

        Assert.Equal(
            new[]
            {
                "SetAnswerAndNext|sid|Mood|One",
                "SetAnswerAndNext|sid|Mood|Two",
                "SetAnswerAndNext|sid|Mood|Three",
                "SetAnswerAndNext|sid|Mood|Four",
                "SetAnswerAndNext|sid|Mood|Five",
                "SetAnswerAndNext|sid|Mood|Six",
                "SetAnswerAndNext|sid|Mood|Seven",
                "SetAnswerAndNext|sid|Mood|Eight",
                "Consultation"
            },
            KeyboardReader.Payloads(keyboard));

        var reset = rows[^1].Single();
        Assert.Equal(VkBotTexts.StartOver, reset.Action.Label);
        Assert.Equal("Negative", reset.Color.ToString());
    }

    [Fact]
    public void QuestionKeyboard_limits_options_to_27()
    {
        var options = Enumerable.Range(1, 30)
            .Select(x => ($"Вариант {x}", $"Value{x}"))
            .ToArray();
        var question = FlowFactory.Question(options: options).Step.Question!;

        var keyboard = factory.CreateQuestionKeyboard("sid", question);
        var rows = KeyboardReader.Rows(keyboard);

        Assert.Equal(new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 1 }, rows.Select(x => x.Count));
        Assert.Equal(28, KeyboardReader.Buttons(keyboard).Count);
        Assert.DoesNotContain(
            KeyboardReader.Payloads(keyboard),
            payload => payload.Contains("Value28", StringComparison.Ordinal));
    }

    [Fact]
    public void FilmKeyboard_has_expected_payloads()
    {
        var keyboard = factory.CreateFilmKeyboard("sid", 42);
        var rows = KeyboardReader.Rows(keyboard);

        Assert.Equal(new[] { 1, 1, 2 }, rows.Select(x => x.Count));
        Assert.Equal(
            new[]
            {
                "Consultation",
                "Another|sid|42",
                "Review|sid|Negative",
                "Review|sid|Positive"
            },
            KeyboardReader.Payloads(keyboard));

        Assert.Equal("Подобрать еще один фильм!", rows[0].Single().Action.Label);
        Assert.Equal("Другой по тем же параметрам", rows[1].Single().Action.Label);
        Assert.Equal(
            "Подобрать еще один фильм!",
            rows.SelectMany(x => x)
                .First(x => x.Action.Payload == "Consultation")
                .Action.Label);
        Assert.Equal(
            "Secondary",
            rows[2][1].Color.ToString());
    }
}

public sealed class FilmMessageFormatterTests
{
    [Fact]
    public void FilmFormatter_matches_old_card()
    {
        var film = new RecommendedFilm
        {
            Id = 1,
            Name = "Фильм &amp; Ко",
            Year = 1999,
            Rating = 7.5m,
            Description = "Описание &quot;фильма&quot;",
            Tags = new[]
            {
                new FilmTag { Name = "Смешной", Percent = 20 },
                new FilmTag { Name = "Грустный", Percent = 80 }
            }
        };

        var text = new FilmMessageFormatter().Format(film);

        Assert.Contains("Фильм & Ко (1999)", text);
        Assert.Contains("Осторожно, возможен машинный перевод!", text);
        Assert.Contains("Описание \"фильма\"", text);
        Assert.Contains($"Рейтинг IMDb: {7.5m:N2}", text);
        Assert.Contains("Основываясь на мнении пользователей, этот фильм: ", text);
        Assert.Contains($"80% Грустный{Environment.NewLine}20% Смешной", text);
    }
}
