using System.Text.Json;
using VkBotApi.Contracts;

namespace VkBotApi.Parsing;

/// <summary>
/// Превращает сообщение в команду. Payload имеет приоритет над текстом;
/// имя команды сравнивается без учёта регистра, аргументы сохраняются как есть.
/// </summary>
public sealed class VkCommandParser
{
    public VkCommandRequest? Parse(VkIncomingMessage message)
    {
        string? source;

        if (!string.IsNullOrWhiteSpace(message.Payload))
        {
            source = ParsePayloadCommand(message.Payload);
            if (string.IsNullOrWhiteSpace(source))
                return null;
        }
        else
        {
            source = message.Text;
        }

        if (string.IsNullOrWhiteSpace(source))
            return null;

        var parts = source.Split('|');
        var name = parts[0].Trim();
        if (name.Length == 0)
            return null;

        var arguments = parts
            .Skip(1)
            .Select(x => x.Trim())
            .ToArray();

        return new VkCommandRequest
        {
            Name = name,
            Arguments = arguments
        };
    }

    private static string? ParsePayloadCommand(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("command", out var command) &&
                command.ValueKind == JsonValueKind.String)
            {
                return command.GetString();
            }

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString();
        }
        catch (JsonException)
        {
            // Битый payload не должен ронять обработку.
        }

        return null;
    }
}
