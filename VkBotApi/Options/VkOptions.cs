namespace VkBotApi.Options;

public sealed class VkOptions
{
    public const string SectionName = "Vk";

    public string AccessToken { get; set; } = string.Empty;
    public string ConfirmationToken { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public long? GroupId { get; set; }
}
