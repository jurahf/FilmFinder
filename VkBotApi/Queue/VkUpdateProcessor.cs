using VkBotApi.Bot;

namespace VkBotApi.Queue;

/// <summary>Обрабатывает сообщения из очереди в отдельном scope на каждое обновление.</summary>
public sealed class VkUpdateProcessor : BackgroundService
{
    private readonly VkUpdateQueue queue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<VkUpdateProcessor> logger;

    public VkUpdateProcessor(
        VkUpdateQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<VkUpdateProcessor> logger)
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
                    var handler = scope.ServiceProvider.GetRequiredService<VkBotHandler>();
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
                        "Ошибка обработки обновления VK для peer {PeerId}.",
                        message.PeerId);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка приложения.
        }
    }
}
