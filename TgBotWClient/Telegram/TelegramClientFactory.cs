using Microsoft.Extensions.Options;
using TgBotWClient.Options;
using WTelegram;

namespace TgBotWClient.Telegram;

/// <summary>
/// Создаёт единственный WTelegram-клиент: путь сессии, каталог, MTProxy.
/// </summary>
public sealed class TelegramClientFactory
{
    private readonly TelegramOptions options;
    private readonly IHostEnvironment environment;
    private readonly ILogger<TelegramClientFactory> logger;

    public TelegramClientFactory(
        IOptions<TelegramOptions> options,
        IHostEnvironment environment,
        ILogger<TelegramClientFactory> logger)
    {
        this.options = options.Value;
        this.environment = environment;
        this.logger = logger;
    }

    public Client Create()
    {
        var sessionPath = Path.IsPathRooted(options.SessionPath)
            ? options.SessionPath
            : Path.Combine(environment.ContentRootPath, options.SessionPath);

        var sessionDirectory = Path.GetDirectoryName(sessionPath);
        if (!string.IsNullOrWhiteSpace(sessionDirectory))
            Directory.CreateDirectory(sessionDirectory);

        var client = new Client(options.ApiId, options.ApiHash, sessionPath);

        if (options.MtProxy.HasAnyValue)
        {
            client.MTProxyUrl = options.MtProxy.BuildUrl();
            logger.LogInformation(
                "Telegram-клиент будет подключаться через MTProxy {Server}:{Port}.",
                options.MtProxy.Server,
                options.MtProxy.Port);
        }
        else
        {
            logger.LogInformation("Telegram-клиент будет подключаться напрямую.");
        }

        logger.LogInformation("Файл сессии Telegram: {SessionPath}.", sessionPath);
        return client;
    }
}
