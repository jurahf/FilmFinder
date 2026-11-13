using TgBotWClient.Bot;

namespace TgBotWClient.Telegram;

/// <summary>Обрабатывает обновления из очереди в отдельном scope на каждое.</summary>
public sealed class TgUpdateProcessor : BackgroundService
{
    private readonly TgUpdateQueue queue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<TgUpdateProcessor> logger;

    public TgUpdateProcessor(
        TgUpdateQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<TgUpdateProcessor> logger)
    {
        this.queue = queue;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TgBotHandler>();
                    await handler.HandleAsync(message, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Ошибка обработки обновления Telegram для чата {ChatId}.",
                        message.ChatId);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка приложения.
        }
    }
}
