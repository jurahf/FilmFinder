namespace VkBotApi.Contracts;

public enum VkCallbackKind
{
    Ignored,
    Confirmation,
    Message
}

public sealed class VkCallbackResult
{
    public VkCallbackKind Kind { get; init; }
    public string? Secret { get; init; }
    public VkIncomingMessage? Message { get; init; }
}
