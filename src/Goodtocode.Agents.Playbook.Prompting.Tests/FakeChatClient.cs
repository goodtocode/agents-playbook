using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting.Tests;

/// <summary>
/// Returns a canned JSON payload for every call, so structured-output tests never make a live
/// model call. Captures the last prompt sent for assertions.
/// </summary>
internal sealed class FakeChatClient(string jsonResponse) : IChatClient
{
    public string? LastPromptText { get; private set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LastPromptText = string.Join(Environment.NewLine, messages.Select(message => message.Text));

        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, jsonResponse));
        return Task.FromResult(response);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Streaming is not used by the prompt-based tools.");
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value);
}
