using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using VkBotApi.Options;
using VkNet;
using VkNet.Model;

namespace VkBotApi.Bot;

public sealed class VkMessageSender : IVkMessageSender
{
    private readonly VkApi api;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly VkOptions options;
    private readonly ILogger<VkMessageSender> logger;

    public VkMessageSender(
        VkApi api,
        IHttpClientFactory httpClientFactory,
        IOptions<VkOptions> options,
        ILogger<VkMessageSender> logger)
    {
        this.api = api;
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task SendTextAsync(
        long peerId,
        string text,
        MessageKeyboard? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        await api.Messages.SendAsync(new MessagesSendParams
        {
            PeerId = peerId,
            Message = text,
            RandomId = Random.Shared.NextInt64(),
            Keyboard = keyboard,
            GroupId = (ulong)(options.GroupId ?? 0)
        }, cancellationToken);
    }

    public async Task SendPhotoAsync(
        long peerId,
        string photoUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            logger.LogWarning("Постер {PhotoUrl} не является абсолютной http(s)-ссылкой.", photoUrl);
            return;
        }

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            var bytes = await httpClient.GetByteArrayAsync(uri, cancellationToken);

            var uploadServer = await api.Photo.GetMessagesUploadServerAsync(
                options.GroupId,
                cancellationToken);

            using var content = new MultipartFormDataContent();
            var photoContent = new ByteArrayContent(bytes);
            photoContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(photoContent, "photo", "poster.jpg");

            using var uploadResponse = await httpClient.PostAsync(
                uploadServer.UploadUrl,
                content,
                cancellationToken);
            uploadResponse.EnsureSuccessStatusCode();

            var uploadResult = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
            var photos = await api.Photo.SaveMessagesPhotoAsync(uploadResult, cancellationToken);
            var photo = photos.FirstOrDefault();

            if (photo == null)
            {
                logger.LogWarning("ВКонтакте не вернул сохранённый постер {PhotoUrl}.", photoUrl);
                return;
            }

            await api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = peerId,
                Message = string.Empty,
                RandomId = Random.Shared.NextInt64(),
                Attachments = new MediaAttachment[] { photo },
                GroupId = (ulong)(options.GroupId ?? 0)
            }, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Не удалось отправить постер {PhotoUrl}.", photoUrl);
        }
    }
}
