using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Goodtocode.Agents.Playbook.Tools;

/// <summary>
/// Stable, validated key used to resolve a registered Collect/Evaluate/Record tool.
/// </summary>
[JsonConverter(typeof(PlaybookToolKeyJsonConverter))]
public readonly partial record struct PlaybookToolKey(string Value)
{
    public static PlaybookToolKey Empty => new(string.Empty);

    public static PlaybookToolKey Create(string? value)
    {
        var normalized = Normalize(value);
        ThrowIfInvalid(normalized, nameof(value));
        return new PlaybookToolKey(normalized);
    }

    public static bool TryCreate(string? value, out PlaybookToolKey result, out string error)
    {
        var normalized = Normalize(value);
        if (!ValidateInternal(normalized, out error))
        {
            result = Empty;
            return false;
        }

        result = new PlaybookToolKey(normalized);
        return true;
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    public bool Validate(out string error) => ValidateInternal(Value, out error);

    public override string ToString() => Value;

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static void ThrowIfInvalid(string value, string paramName)
    {
        if (!ValidateInternal(value, out var error))
        {
            throw new ArgumentException(error, paramName);
        }
    }

    private static bool ValidateInternal(string value, out string error)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Playbook tool key is required.";
            return false;
        }

        if (!PlaybookToolTokenPattern().IsMatch(value))
        {
            error = "Playbook tool key must contain only lowercase letters, numbers, hyphen, underscore, or dot.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    [GeneratedRegex("^[a-z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex PlaybookToolTokenPattern();
}

public sealed class PlaybookToolKeyJsonConverter : JsonConverter<PlaybookToolKey>
{
    public override PlaybookToolKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return PlaybookToolKey.Create(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, PlaybookToolKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
