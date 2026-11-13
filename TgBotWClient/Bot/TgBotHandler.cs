using TgBotWClient.Bot.Commands;
using TgBotWClient.Contracts;
using TgBotWClient.Parsing;

namespace TgBotWClient.Bot;

/// <summary>
/// Разбирает обновление, находит команду и обрабатывает её.
/// Callback всегда подтверждается, иначе у пользователя висит «часики».
/// </summary>
public sealed class TgBotHandler
{
    private readonly IEnumerable<ITgCommand> commands;
    private readonly TgCommandParser parser;
    private readonly ITgMessageSender sender;
    private readonly TgKeyboardFactory keyboards;
    private readonly ILogger<TgBotHandler> logger;

    public TgBotHandler(
        IEnumerable<ITgCommand> commands,
        TgCommandParser parser,
        ITgMessageSender sender,
        TgKeyboardFactory keyboards,
        ILogger<TgBotHandler> logger)
    {
        this.commands = commands;
        this.parser = parser;
        this.sender = sender;
        this.keyboards = keyboards;
        this.logger = logger;
    }

    public async Task HandleAsync(TgIncomingMessage message, CancellationToken cancellationToken)
    {
        var request = parser.Parse(message);

        try
        {
            if (request == null)
            {
                await SendUnknownAsync(message, cancellationToken);
                return;
            }

            var command = commands.FirstOrDefault(x =>
                string.Equals(x.Name, request.Name, StringComparison.OrdinalIgnoreCase));

            if (command == null)
            {
                await SendUnknownAsync(message, cancellationToken);
                return;
            }

            logger.LogInformation(
                "Команда {Command} от чата {ChatId} (callback: {IsCallback}).",
                request.Name,
                message.ChatId,
                message.IsCallback);

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
                "Ошибка выполнения команды {Command} для чата {ChatId}.",
                request?.Name ?? "<unknown>",
                message.ChatId);

            await sender.SendTextAsync(
                message.ChatId,
                TgBotTexts.Error,
                keyboards.CreateStartKeyboard(),
                cancellationToken);
        }
        finally
        {
            if (message.IsCallback)
                await AckCallbackAsync(message);
        }
    }

    private async Task SendUnknownAsync(
        TgIncomingMessage message,
        CancellationToken cancellationToken)
    {
        // На неизвестный callback достаточно ack без сообщения.
        if (message.IsCallback)
            return;

        await sender.SendTextAsync(
            message.ChatId,
            TgBotTexts.UnknownCommand,
            keyboards.CreateStartKeyboard(),
            cancellationToken);
    }

    private async Task AckCallbackAsync(TgIncomingMessage message)
    {
        try
        {
            await sender.AnswerCallbackAsync(message.CallbackQueryId, cancellationToken: CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Не удалось подтвердить callback {QueryId} для чата {ChatId}.",
                message.CallbackQueryId,
                message.ChatId);
        }
    }
}
