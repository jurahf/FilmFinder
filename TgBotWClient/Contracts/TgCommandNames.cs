namespace TgBotWClient.Contracts;

/// <summary>Имена команд, общие для парсера и обработчиков.</summary>
public static class TgCommandNames
{
    public const string Start = "start";
    public const string Consultation = "consultation";
    public const string Answer = "answer";
    public const string Another = "another";
    public const string Review = "review";
}

/// <summary>Значения оценки фильма (совместимы с VkBotApi и EsDb.Reviews).</summary>
public static class TgEstimation
{
    public const string Positive = "Positive";
    public const string Negative = "Negative";
}
