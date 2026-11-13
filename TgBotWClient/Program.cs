using EsConsultationServices;
using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsConsultationServices.Workflow;
using EsDb;
using FilmDb;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TgBotWClient.Bot;
using TgBotWClient.Bot.Commands;
using TgBotWClient.Contracts;
using TgBotWClient.Options;
using TgBotWClient.Parsing;
using TgBotWClient.Telegram;
using WTelegram;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // Консольное приложение: конфигурация и файл сессии лежат рядом с исполняемым файлом.
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);

    var builder = Host.CreateApplicationBuilder(args);

    // Локальные секреты перекрывают appsettings.json; файл не хранится в git.
    builder.Configuration.AddJsonFile("secret.appsettings.json", optional: true, reloadOnChange: false);

    var esConnectionString = builder.Configuration.GetConnectionString("EsConnection")
        ?? throw new InvalidOperationException("Не найдена строка подключения 'EsConnection'.");
    var filmConnectionString = builder.Configuration.GetConnectionString("FilmConnection")
        ?? throw new InvalidOperationException("Не найдена строка подключения 'FilmConnection'.");

    var telegramOptions = builder.Configuration
        .GetSection(TelegramOptions.SectionName)
        .Get<TelegramOptions>() ?? new TelegramOptions();
    telegramOptions.Validate();

    var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
    Log.Logger = logger;

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog(logger, dispose: true);

    builder.Services.Configure<ConsultationOptions>(
        builder.Configuration.GetSection(ConsultationOptions.SectionName));
    builder.Services.Configure<TelegramOptions>(
        builder.Configuration.GetSection(TelegramOptions.SectionName));

    builder.Services.AddDbContext<EsDbContext>(options => options.UseSqlServer(esConnectionString));
    builder.Services.AddDbContext<FilmDbContext>(options => options.UseSqlServer(filmConnectionString));

    // Провайдер описаний советов читает FilmDb, поэтому регистрируется до AddEsConsultationServices.
    builder.Services.AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>();
    builder.Services.AddEsConsultationServices();

    // WTelegram-клиент единственный на процесс; контейнер вызовет Dispose.
    builder.Services.AddSingleton<TelegramClientFactory>();
    builder.Services.AddSingleton(provider =>
        provider.GetRequiredService<TelegramClientFactory>().Create());

    builder.Services.AddSingleton<TgUpdateQueue>();
    builder.Services.AddHostedService<TgUpdateProcessor>();
    builder.Services.AddHostedService<TelegramClientHost>();

    builder.Services.AddScoped<TgCommandParser>();
    builder.Services.AddScoped<TgKeyboardFactory>();
    builder.Services.AddScoped<FilmMessageFormatter>();
    builder.Services.AddScoped<TgFilmPresenter>();
    builder.Services.AddScoped<TgStepPresenter>();
    builder.Services.AddScoped<TgBotHandler>();
    builder.Services.AddScoped<ITgMessageSender, TgMessageSender>();
    builder.Services.AddScoped<ITgChatSessionStore, TgChatSessionStore>();

    builder.Services.AddScoped<ITgCommand, StartCommand>();
    builder.Services.AddScoped<ITgCommand, StartConsultationCommand>();
    builder.Services.AddScoped<ITgCommand, SetAnswerAndNextCommand>();
    builder.Services.AddScoped<ITgCommand, AnotherCommand>();
    builder.Services.AddScoped<ITgCommand, ReviewCommand>();

    var app = builder.Build();

    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    Helpers.Log = (level, message) =>
    {
        var telegramLogger = loggerFactory.CreateLogger("WTelegram");
        telegramLogger.Log(MapWTelegramLevel(level), "{Message}", message);
    };

    logger.Information("TgBotWClient запускается.");
    await app.RunAsync();
    logger.Information("TgBotWClient остановлен.");
}
catch (Exception exception)
{
    Log.Fatal(exception, "TgBotWClient не запустился.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

static LogLevel MapWTelegramLevel(int level)
{
    return level switch
    {
        <= 1 => LogLevel.Error,
        2 => LogLevel.Warning,
        3 => LogLevel.Information,
        4 => LogLevel.Debug,
        _ => LogLevel.Trace
    };
}
