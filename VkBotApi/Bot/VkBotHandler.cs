using VkBotApi.Contracts;
using VkBotApi.Parsing;

namespace VkBotApi.Bot;

/// <summary>
/// Разбирает сообщение, находит команду и обрабатывает её.
/// Исключения команд превращаются в понятное сообщение пользователю.
/// </summary>
public sealed class VkBotHandler
{
    private readonly IEnumerable<Commands.IVkCommand> commands;
    private readonly VkCommandParser parser;
    private readonly IVkMessageSender sender;
    private readonly VkKeyboardFactory keyboards;
    private readonly ILogger<VkBotHandler> logger;

    public VkBotHandler(
        IEnumerable<Commands.IVkCommand> commands,
        VkCommandParser parser,
        IVkMessageSender sender,
        VkKeyboardFactory keyboards,
        ILogger<VkBotHandler> logger)
    {
        this.commands = commands;
        this.parser = parser;
        this.sender = sender;
        this.keyboards = keyboards;
        this.logger = logger;
    }

    public async Task HandleAsync(VkIncomingMessage message, CancellationToken cancellationToken)
    {
        var request = parser.Parse(message);
        if (request == null)
            return;

        var command = commands.FirstOrDefault(x =>
            string.Equals(x.Name, request.Name, StringComparison.OrdinalIgnoreCase));

        if (command == null)
            return;

        try
        {
            await command.ExecuteAsync(message, request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Ошибка выполнения команды {Command} для peer {PeerId}.",
                request.Name,
                message.PeerId);

            await sender.SendTextAsync(
                message.PeerId,
                VkBotTexts.Error,
                keyboards.CreateRetryKeyboard(),
                cancellationToken);
        }
    }
}
