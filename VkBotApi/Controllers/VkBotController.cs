using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VkBotApi.Contracts;
using VkBotApi.Options;
using VkBotApi.Parsing;
using VkBotApi.Queue;

namespace VkBotApi.Controllers;

[ApiController]
[Route("api/VkBot")]
public sealed class VkBotController : ControllerBase
{
    private readonly VkCallbackParser parser;
    private readonly VkUpdateQueue queue;
    private readonly VkOptions options;

    public VkBotController(
        VkCallbackParser parser,
        VkUpdateQueue queue,
        IOptions<VkOptions> options)
    {
        this.parser = parser;
        this.queue = queue;
        this.options = options.Value;
    }

    /// <summary>
    /// Принимает callback ВКонтакте. Сообщение ставится в очередь,
    /// ответ ВКонтакте отправляется сразу.
    /// </summary>
    [HttpPost("VkMessageAccept")]
    public async Task<IActionResult> VkMessageAccept(
        [FromBody] JsonElement update,
        CancellationToken cancellationToken)
    {
        var parsed = parser.Parse(update);

        if (!string.IsNullOrWhiteSpace(options.Secret) &&
            !string.Equals(options.Secret, parsed.Secret, StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (parsed.Kind == VkCallbackKind.Confirmation)
            return Content(options.ConfirmationToken, "text/plain");

        if (parsed.Kind == VkCallbackKind.Message && parsed.Message != null)
            await queue.EnqueueAsync(parsed.Message, cancellationToken);

        return Content("ok", "text/plain");
    }
}
