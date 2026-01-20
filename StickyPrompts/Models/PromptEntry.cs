using System.Text.Json.Serialization;

namespace StickyPrompts.Models;

/// <summary>
/// Represents a single prompt entry with metadata for display and tracking.
/// </summary>
public record PromptEntry(
    Guid Id,
    string Title,
    string Content,
    string ColorHex,
    DateTime LastUsed
)
{
    /// <summary>
    /// Creates a new PromptEntry with a fresh GUID and current timestamp.
    /// </summary>
    public static PromptEntry Create(string title, string content, string colorHex) =>
        new(Guid.NewGuid(), title, content, colorHex, DateTime.UtcNow);

    /// <summary>
    /// Returns a copy with updated LastUsed timestamp.
    /// </summary>
    public PromptEntry WithUpdatedLastUsed() =>
        this with { LastUsed = DateTime.UtcNow };
}

/// <summary>
/// JSON source generation context for AOT-friendly serialization.
/// </summary>
[JsonSerializable(typeof(List<PromptEntry>))]
[JsonSerializable(typeof(PromptEntry))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
public partial class PromptJsonContext : JsonSerializerContext
{
}
