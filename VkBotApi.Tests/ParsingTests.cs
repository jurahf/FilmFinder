using System.Text.Json;
using VkBotApi.Contracts;
using VkBotApi.Parsing;
using Xunit;

namespace VkBotApi.Tests;

public sealed class CallbackParserTests
{
    private readonly VkCallbackParser parser = new();

    [Fact]
    public void CallbackParser_reads_modern_object_message()
    {
        const string json = """
        {
          "type": "message_new",
          "secret": "s3",
          "object": {
            "message": {
              "id": 10,
              "peer_id": 20,
              "from_id": 20,
              "text": "start",
              "payload": "{\"command\":\"Consultation\"}"
            }
          },
          "group_id": 30
        }
        """;

        var result = parser.Parse(JsonDocument.Parse(json).RootElement);

        Assert.Equal(VkCallbackKind.Message, result.Kind);
        Assert.Equal("s3", result.Secret);
        Assert.NotNull(result.Message);
        Assert.Equal(20, result.Message!.PeerId);
        Assert.Equal(20, result.Message.FromId);
        Assert.Equal(10, result.Message.MessageId);
        Assert.Equal("start", result.Message.Text);
        Assert.Equal("{\"command\":\"Consultation\"}", result.Message.Payload);
    }

    [Fact]
    public void CallbackParser_reads_legacy_flat_object()
    {
        const string json = """
        {
          "type": "message_new",
          "object": {
            "peer_id": 20,
            "from_id": 21,
            "text": "Consultation",
            "payload": "{\"command\":\"Consultation\"}"
          }
        }
        """;

        var result = parser.Parse(JsonDocument.Parse(json).RootElement);

        Assert.Equal(VkCallbackKind.Message, result.Kind);
        Assert.NotNull(result.Message);
        Assert.Equal(20, result.Message!.PeerId);
        Assert.Equal(21, result.Message.FromId);
        Assert.Equal(0, result.Message.MessageId);
        Assert.Equal("Consultation", result.Message.Text);
    }

    [Fact]
    public void CallbackParser_reads_confirmation()
    {
        const string json = """
        { "type": "confirmation", "group_id": 30 }
        """;

        var result = parser.Parse(JsonDocument.Parse(json).RootElement);

        Assert.Equal(VkCallbackKind.Confirmation, result.Kind);
        Assert.Null(result.Message);
    }

    [Fact]
    public void CallbackParser_ignores_unknown_types_and_broken_shapes()
    {
        Assert.Equal(
            VkCallbackKind.Ignored,
            parser.Parse(JsonDocument.Parse("""{ "type": "wall_post_new" }""").RootElement).Kind);

        Assert.Equal(
            VkCallbackKind.Ignored,
            parser.Parse(JsonDocument.Parse("""{ "type": "message_new", "object": {} }""").RootElement).Kind);

        Assert.Equal(
            VkCallbackKind.Ignored,
            parser.Parse(JsonDocument.Parse("[]").RootElement).Kind);
    }
}

public sealed class CommandParserTests
{
    private readonly VkCommandParser parser = new();

    [Fact]
    public void CommandParser_prefers_payload_over_text()
    {
        var message = new VkIncomingMessage
        {
            PeerId = 1,
            Text = "start",
            Payload = "{\"command\":\"SetAnswerAndNext|sid|Mood|Good\"}"
        };

        var request = parser.Parse(message);

        Assert.NotNull(request);
        Assert.Equal("SetAnswerAndNext", request!.Name);
        Assert.Equal(new[] { "sid", "Mood", "Good" }, request.Arguments);
    }

    [Fact]
    public void CommandParser_preserves_argument_case()
    {
        var message = new VkIncomingMessage
        {
            PeerId = 1,
            Payload = "{\"command\":\"SetAnswerAndNext|AbC123|Вопрос|Йоркширский терьер\"}"
        };

        var request = parser.Parse(message);

        Assert.NotNull(request);
        Assert.Equal("SetAnswerAndNext", request!.Name);
        Assert.Equal("AbC123", request.GetArgument(0));
        Assert.Equal("Вопрос", request.GetArgument(1));
        Assert.Equal("Йоркширский терьер", request.GetArgument(2));
    }

    [Fact]
    public void CommandParser_falls_back_to_text_and_reads_string_payload()
    {
        var fromText = parser.Parse(new VkIncomingMessage { PeerId = 1, Text = "Consultation" });
        Assert.NotNull(fromText);
        Assert.Equal("Consultation", fromText!.Name);

        var fromStringPayload = parser.Parse(new VkIncomingMessage
        {
            PeerId = 1,
            Text = "ignored",
            Payload = "\"Another|sid|42\""
        });
        Assert.NotNull(fromStringPayload);
        Assert.Equal("Another", fromStringPayload!.Name);
        Assert.Equal("42", fromStringPayload.GetArgument(1));
    }

    [Fact]
    public void CommandParser_returns_null_for_missing_or_broken_command()
    {
        Assert.Null(parser.Parse(new VkIncomingMessage { PeerId = 1 }));
        Assert.Null(parser.Parse(new VkIncomingMessage { PeerId = 1, Text = "   " }));
        Assert.Null(parser.Parse(new VkIncomingMessage
        {
            PeerId = 1,
            Text = "start",
            Payload = "{not json"
        }));
        Assert.Null(parser.Parse(new VkIncomingMessage
        {
            PeerId = 1,
            Text = "start",
            Payload = "{\"command\":123}"
        }));
        Assert.Null(parser.Parse(new VkIncomingMessage { PeerId = 1, Text = "|x" }));
    }
}
