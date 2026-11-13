namespace TgBotWClient.Options;

/// <summary>
/// Настройки подключения бота к Telegram (MTProto через WTelegramClient).
/// Секреты (ApiHash, BotToken, секрет MTProxy) задаются в secret.appsettings.json.
/// </summary>
public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    /// <summary>api_id приложения с my.telegram.org.</summary>
    public int ApiId { get; set; }

    /// <summary>api_hash приложения с my.telegram.org.</summary>
    public string ApiHash { get; set; } = string.Empty;

    /// <summary>Токен бота от @BotFather.</summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>Путь к файлу сессии WTelegram; относительный — от каталога приложения.</summary>
    public string SessionPath { get; set; } = "App_Data/telegram.session";

    public MtProxyOptions MtProxy { get; set; } = new();

    public void Validate()
    {
        if (ApiId <= 0)
            throw new InvalidOperationException("Не задан Telegram:ApiId (api_id с my.telegram.org).");

        if (string.IsNullOrWhiteSpace(ApiHash))
            throw new InvalidOperationException("Не задан Telegram:ApiHash (api_hash с my.telegram.org).");

        if (string.IsNullOrWhiteSpace(BotToken))
            throw new InvalidOperationException("Не задан Telegram:BotToken (токен от @BotFather).");

        if (string.IsNullOrWhiteSpace(SessionPath))
            throw new InvalidOperationException("Не задан Telegram:SessionPath.");

        MtProxy.Validate();
    }
}

/// <summary>
/// Настройки MTProxy. Пустые значения означают прямое подключение;
/// частично заполненная секция — ошибка конфигурации.
/// </summary>
public sealed class MtProxyOptions
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Secret { get; set; } = string.Empty;

    public bool HasAnyValue =>
        !string.IsNullOrWhiteSpace(Server) ||
        Port != 0 ||
        !string.IsNullOrWhiteSpace(Secret);

    public void Validate()
    {
        if (!HasAnyValue)
            return;

        if (string.IsNullOrWhiteSpace(Server) || Port <= 0 || string.IsNullOrWhiteSpace(Secret))
            throw new InvalidOperationException(
                "Telegram:MtProxy заполнен частично: нужны Server, Port и Secret.");
    }

    public string BuildUrl()
    {
        return "https://t.me/proxy" +
            $"?server={Uri.EscapeDataString(Server)}" +
            $"&port={Port}" +
            $"&secret={Uri.EscapeDataString(Secret)}";
    }
}
