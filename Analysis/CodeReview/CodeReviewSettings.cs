using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Stores the editable code review rule settings for one solution or project directory.
/// </summary>
public sealed class CodeReviewSettings
{
    public const string ConfigurationFileName = ".code-review.json";
    public const string SyntaxDiagnosticsRule = "CSharp.SyntaxDiagnostics";
    public const string ConstantNamingRule = "CSharp.ConstantNaming";
    public const string NullDefaultRule = "CSharp.NullDefault";
    public const string EmptyCatchRule = "CSharp.EmptyCatch";
    public const string UnbracedIfRule = "CSharp.UnbracedIf";
    public const string DeferredWorkRule = "CSharp.DeferredWork";
    public const string MigrationGuardRule = "Sql.MigrationExistenceGuard";
    public const string AsyncVoidRule = "CSharp.AsyncVoid";
    public const string AsyncWithoutAwaitRule = "CSharp.AsyncWithoutAwait";
    public const string ThrowExceptionVariableRule = "CSharp.ThrowExceptionVariable";
    public const string SyncOverAsyncRule = "CSharp.SyncOverAsync";
    public const string InterpolatedLogRule = "CSharp.InterpolatedLog";
    public const string LogExceptionMessageRule = "CSharp.LogExceptionMessage";
    public const string LogAndRethrowRule = "CSharp.LogAndRethrow";
    public const string DirectClockRule = "CSharp.DirectClock";
    public const string UnbracedLoopRule = "CSharp.UnbracedLoop";
    public const string UtcFieldLocalTimeRule = "CSharp.UtcFieldLocalTime";
    public const string MultipleDatabaseWritesRule = "CSharp.MultipleDatabaseWrites";
    public const string NullSuppressionRule = "CSharp.NullSuppression";
    public const string InlineDataArrayRule = "CSharp.InlineDataArray";
    public const string SqlUtcTimestampRule = "Sql.UtcTimestamp";
    public const string SqlNamedPrimaryKeyRule = "Sql.NamedPrimaryKey";

    private const RegexOptions SafeRegexOptions = RegexOptions.CultureInvariant | RegexOptions.NonBacktracking;

    private static readonly IReadOnlyDictionary<string, CodeReviewRuleSettings> DefaultRules =
        new Dictionary<string, CodeReviewRuleSettings>(StringComparer.Ordinal)
        {
            [SyntaxDiagnosticsRule] = new(),
            [ConstantNamingRule] = new() { Pattern = @"^[A-Z][A-Z0-9_]*$" },
            [NullDefaultRule] = new(),
            [EmptyCatchRule] = new(),
            [UnbracedIfRule] = new(),
            [DeferredWorkRule] = new() { Pattern = @"\b(TODO|FIXME)\b" },
            [MigrationGuardRule] = new() { Pattern = @"\bIF\s+(NOT\s+)?EXISTS\b" },
            [AsyncVoidRule] = new(),
            [AsyncWithoutAwaitRule] = new(),
            [ThrowExceptionVariableRule] = new(),
            [SyncOverAsyncRule] = new(),
            [InterpolatedLogRule] = new(),
            [LogExceptionMessageRule] = new(),
            [LogAndRethrowRule] = new(),
            [DirectClockRule] = new(),
            [UnbracedLoopRule] = new(),
            [UtcFieldLocalTimeRule] = new(),
            [MultipleDatabaseWritesRule] = new(),
            [NullSuppressionRule] = new(),
            [InlineDataArrayRule] = new(),
            [SqlUtcTimestampRule] = new(),
            [SqlNamedPrimaryKeyRule] = new(),
        };

    /// <summary>
    /// Gets or sets the settings indexed by stable rule ID.
    /// </summary>
    [JsonPropertyName("rules")]
    public Dictionary<string, CodeReviewRuleSettings> Rules { get; set; } = CreateDefaultRules();

    /// <summary>
    /// Gets settings for a known rule, including defaults for omitted entries.
    /// </summary>
    public CodeReviewRuleSettings GetRule(string ruleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        if (Rules.TryGetValue(ruleId, out CodeReviewRuleSettings? settings))
        {
            return settings;
        }

        return DefaultRules.TryGetValue(ruleId, out CodeReviewRuleSettings? defaults)
            ? defaults
            : throw new ArgumentOutOfRangeException(nameof(ruleId), ruleId, "Unknown code review rule.");
    }

    /// <summary>
    /// Creates a settings instance containing all built-in rule defaults.
    /// </summary>
    public static CodeReviewSettings CreateDefault() => new();

    /// <summary>
    /// Validates rule IDs, values, and regular expressions in the settings.
    /// </summary>
    public void Validate()
    {
        if (Rules is null)
        {
            throw new InvalidDataException("The 'rules' object cannot be null.");
        }

        foreach ((string ruleId, CodeReviewRuleSettings? settings) in Rules)
        {
            if (!DefaultRules.ContainsKey(ruleId))
            {
                throw new InvalidDataException($"Unknown code review rule '{ruleId}'. Check the rule ID spelling or application version.");
            }

            if (settings is null)
            {
                throw new InvalidDataException($"Settings for rule '{ruleId}' cannot be null.");
            }

            if (settings.Severity is ReviewSeverity severity && !Enum.IsDefined(severity))
            {
                throw new InvalidDataException($"Rule '{ruleId}' has an unsupported severity.");
            }

            if (settings.Pattern is not null)
            {
                try
                {
                    _ = new Regex(settings.Pattern, SafeRegexOptions, TimeSpan.FromMilliseconds(500));
                }
                catch (ArgumentException exception)
                {
                    throw new InvalidDataException($"Rule '{ruleId}' has an invalid regular expression: {exception.Message}", exception);
                }
            }
        }

        foreach ((string ruleId, CodeReviewRuleSettings defaults) in DefaultRules)
        {
            CodeReviewRuleSettings settings = GetRule(ruleId);
            if (defaults.Pattern is not null && settings.Pattern is null)
            {
                settings.Pattern = defaults.Pattern;
            }
            else if (defaults.Pattern is not null && string.IsNullOrWhiteSpace(settings.Pattern))
            {
                throw new InvalidDataException($"Rule '{ruleId}' requires a non-empty pattern.");
            }

            if (defaults.Pattern is null && settings.Pattern is not null)
            {
                throw new InvalidDataException($"Rule '{ruleId}' does not support a custom pattern.");
            }
        }
    }

    private static Dictionary<string, CodeReviewRuleSettings> CreateDefaultRules()
    {
        return DefaultRules.ToDictionary(
            entry => entry.Key,
            entry => new CodeReviewRuleSettings
            {
                Enabled = entry.Value.Enabled,
                Severity = entry.Value.Severity,
                Pattern = entry.Value.Pattern,
            },
            StringComparer.Ordinal);
    }
}
