namespace VkBotApi.Contracts;

public sealed class VkIncomingMessage
{
    public long PeerId { get; init; }
    public long FromId { get; init; }
    public long MessageId { get; init; }
    public string? Text { get; init; }
    public string? Payload { get; init; }
}
