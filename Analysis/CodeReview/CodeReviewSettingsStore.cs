using System.Text.Json;
using System.Text.Json.Serialization;

namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Loads and creates the per-solution code review configuration file.
/// </summary>
public static class CodeReviewSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    /// <summary>
    /// Gets the configuration path for a solution or project directory.
    /// </summary>
    public static string GetConfigurationPath(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        return Path.Combine(Path.GetFullPath(directory), CodeReviewSettings.ConfigurationFileName);
    }

    /// <summary>
    /// Loads configuration, returning built-in defaults when no file exists.
    /// </summary>
    public static async Task<CodeReviewSettings> LoadAsync(string directory, CancellationToken cancellationToken = default)
    {
        string path = GetConfigurationPath(directory);
        if (!File.Exists(path))
        {
            return CodeReviewSettings.CreateDefault();
        }

        try
        {
            await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            CodeReviewSettings? settings = await JsonSerializer.DeserializeAsync<CodeReviewSettings>(stream, SerializerOptions, cancellationToken);
            if (settings is null)
            {
                throw new InvalidDataException($"The code review configuration '{path}' is empty or does not contain a JSON object.");
            }

            settings.Validate();
            return settings;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The code review configuration '{path}' contains invalid JSON: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Creates a default configuration file without overwriting an existing file.
    /// </summary>
    public static async Task<string> CreateDefaultFileAsync(string directory, CancellationToken cancellationToken = default)
    {
        string path = GetConfigurationPath(directory);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        try
        {
            await using FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, CodeReviewSettings.CreateDefault(), SerializerOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch (IOException) when (File.Exists(path))
        {
        }

        return path;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        return options;
    }
}
