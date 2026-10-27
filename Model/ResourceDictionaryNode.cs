namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a merged resource dictionary.
/// </summary>
public sealed class ResourceDictionaryNode
{
    /// <summary>
    /// Gets or initializes the source URI.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets child merged dictionaries.
    /// </summary>
    public List<ResourceDictionaryNode> MergedDictionaries { get; } = [];

    /// <summary>
    /// Gets resource keys declared by the dictionary.
    /// </summary>
    public List<string> ResourceKeys { get; } = [];
}
