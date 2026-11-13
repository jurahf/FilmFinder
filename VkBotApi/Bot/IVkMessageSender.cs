using VkNet.Model;

namespace VkBotApi.Bot;

public interface IVkMessageSender
{
    Task SendTextAsync(
        long peerId,
        string text,
        MessageKeyboard? keyboard = null,
        CancellationToken cancellationToken = default);

    Task SendPhotoAsync(
        long peerId,
        string photoUrl,
        CancellationToken cancellationToken = default);
}
