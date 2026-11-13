namespace TgBotWClient.Bot;

public static class TgBotTexts
{
    public const string Hello =
        "Привет! Я помогу подобрать фильм под ситуацию и настроение.\n\n" +
        "Что я умею:\n" +
        "• «Подобрать фильм» — короткая консультация и рекомендация.\n" +
        "• «Другой фильм» — ещё один вариант по тем же ответам.\n" +
        "• «👍 Ок» / «👎 Не ок» — оценить показанный фильм.\n" +
        "• «Начать заново» — новая консультация.\n\n" +
        "Команды: /start, /help, /consultation, /another.";

    public const string Started = "Начинаем подбор. Отвечайте на вопросы кнопками под сообщением.";
    public const string SearchingFilm = "Сейчас что-нибудь подберу...";
    public const string SearchingAnother = "Сейчас поищу...";
    public const string ThanksForReview = "Спасибо за оценку!";

    public const string NotFound = "К сожалению, по данным параметрам ничего не найдено.";
    public const string NoMoreFilms = "Больше нет подходящих фильмов. Можно начать подбор заново.";
    public const string NotCompleted = "Сначала нужно ответить на все вопросы консультации.";
    public const string NoBinding =
        "Не вижу активной консультации. Нажмите «Подобрать фильм», чтобы начать новую.";
    public const string StaleQuestion = "Этот вопрос уже не актуален. Показываю текущий шаг.";
    public const string UnknownCommand =
        "Не понял команду. Воспользуйтесь кнопками ниже или отправьте /start.";
    public const string Error =
        "К сожалению, в работе сервиса произошла ошибка. Вы можете повторить попытку позже.";

    // Тексты reply-кнопок. По ним же парсер распознаёт нажатия.
    public const string StartConsultationButton = "Подобрать фильм";
    public const string AnotherButton = "Другой фильм";
    public const string StartOverButton = "Начать заново";
    public const string LikeButton = "👍 Ок";
    public const string DislikeButton = "👎 Не ок";
}
