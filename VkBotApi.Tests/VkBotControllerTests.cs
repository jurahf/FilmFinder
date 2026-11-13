using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VkBotApi.Controllers;
using VkBotApi.Options;
using VkBotApi.Parsing;
using VkBotApi.Queue;
using Xunit;

namespace VkBotApi.Tests;

public sealed class VkBotControllerTests
{
    private static VkBotController CreateController(
        VkUpdateQueue queue,
        string confirmationToken = "confirm",
        string? secret = null)
    {
        return new VkBotController(
            new VkCallbackParser(),
            queue,
            Microsoft.Extensions.Options.Options.Create(new VkOptions
            {
                ConfirmationToken = confirmationToken,
                Secret = secret
            }));
    }

    [Fact]
    public async Task Controller_returns_confirmation_token()
    {
        var controller = CreateController(new VkUpdateQueue());

        var action = await controller.VkMessageAccept(
            JsonDocument.Parse("""{ "type": "confirmation" }""").RootElement,
            CancellationToken.None);

        var content = Assert.IsType<ContentResult>(action);
        Assert.Equal("confirm", content.Content);
        Assert.Equal("text/plain", content.ContentType);
    }

    [Fact]
    public async Task Controller_rejects_wrong_secret()
    {
        var queue = new VkUpdateQueue();
        var controller = CreateController(queue, secret: "expected");

        var action = await controller.VkMessageAccept(
            JsonDocument.Parse("""{ "type": "message_new", "secret": "other" }""").RootElement,
            CancellationToken.None);

        var status = Assert.IsType<StatusCodeResult>(action);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task Controller_enqueues_message_and_replies_ok()
    {
        var queue = new VkUpdateQueue();
        var controller = CreateController(queue, secret: "expected");

        var action = await controller.VkMessageAccept(
            JsonDocument.Parse("""
            {
              "type": "message_new",
              "secret": "expected",
              "object": { "message": { "peer_id": 5, "text": "start" } }
            }
            """).RootElement,
            CancellationToken.None);

        var content = Assert.IsType<ContentResult>(action);
        Assert.Equal("ok", content.Content);

        await using var enumerator = queue
            .ReadAllAsync(CancellationToken.None)
            .GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(5, enumerator.Current.PeerId);
        Assert.Equal("start", enumerator.Current.Text);
    }
}
