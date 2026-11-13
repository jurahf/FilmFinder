using System.Text.Json;
using VkBotApi.Contracts;

namespace VkBotApi.Parsing;

/// <summary>
/// Разбирает callback ВКонтакте: confirmation и message_new в современном
/// (object.message) и старом плоском (object) форматах.
/// </summary>
public sealed class VkCallbackParser
{
    public VkCallbackResult Parse(JsonElement update)
    {
        if (update.ValueKind != JsonValueKind.Object)
            return new VkCallbackResult { Kind = VkCallbackKind.Ignored };

        var type = GetString(update, "type");
        var secret = GetString(update, "secret");

        if (string.Equals(type, "confirmation", StringComparison.OrdinalIgnoreCase))
            return new VkCallbackResult { Kind = VkCallbackKind.Confirmation, Secret = secret };

        if (!string.Equals(type, "message_new", StringComparison.OrdinalIgnoreCase))
            return new VkCallbackResult { Kind = VkCallbackKind.Ignored, Secret = secret };

        if (!update.TryGetProperty("object", out var callbackObject) ||
            callbackObject.ValueKind != JsonValueKind.Object)
            return new VkCallbackResult { Kind = VkCallbackKind.Ignored, Secret = secret };

        var message = callbackObject;
        if (callbackObject.TryGetProperty("message", out var nestedMessage) &&
            nestedMessage.ValueKind == JsonValueKind.Object)
        {
            message = nestedMessage;
        }

        var peerId = GetLong(message, "peer_id");
        if (peerId == null)
            return new VkCallbackResult { Kind = VkCallbackKind.Ignored, Secret = secret };

        return new VkCallbackResult
        {
            Kind = VkCallbackKind.Message,
            Secret = secret,
            Message = new VkIncomingMessage
            {
                PeerId = peerId.Value,
                FromId = GetLong(message, "from_id") ?? 0,
                MessageId = GetLong(message, "id") ?? 0,
                Text = GetString(message, "text"),
                Payload = GetPayload(message)
            }
        };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }

    private static long? GetLong(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var number))
            return number;

        if (property.ValueKind == JsonValueKind.String &&
            long.TryParse(property.GetString(), out number))
            return number;

        return null;
    }

    private static string? GetPayload(JsonElement message)
    {
        if (!message.TryGetProperty("payload", out var payload))
            return null;

        return payload.ValueKind switch
        {
            JsonValueKind.String => payload.GetString(),
            JsonValueKind.Object or JsonValueKind.Array => payload.GetRawText(),
            _ => null
        };
    }
}
