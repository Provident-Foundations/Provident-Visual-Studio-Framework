namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Infers naming patterns and evaluates observations against them.</summary>
public sealed class NamingPatternAnalyzer
{
    /// <summary>Infers patterns for the supplied observations.</summary>
    public IReadOnlyList<NamingPattern> InferPatterns(IReadOnlyList<NamingObservation> observations, NamingAnalysisConfiguration configuration, string? projectName = null)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(configuration);
        return observations.Where(item => !item.IsExcluded)
            .GroupBy(item => new { item.Kind, item.Language })
            .Select(group => CreatePattern(group.Key.Kind, group.Key.Language, projectName, group.ToList(), configuration))
            .ToArray();
    }

    /// <summary>Evaluates observations against inferred patterns.</summary>
    public IReadOnlyList<NamingViolation> FindViolations(IReadOnlyList<NamingObservation> observations, IReadOnlyList<NamingPattern> patterns, NamingAnalysisConfiguration configuration, IReadOnlyList<NamingSuppression>? suppressions = null)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(patterns);
        ArgumentNullException.ThrowIfNull(configuration);
        List<NamingViolation> violations = new();
        foreach (NamingPattern pattern in patterns.Where(item => !item.HasInsufficientEvidence && !item.IsMixed))
        {
            HashSet<string> members = pattern.ObservationIds.ToHashSet(StringComparer.Ordinal);
            foreach (NamingObservation observation in observations.Where(item => members.Contains(item.Id)))
            {
                bool styleMismatch = observation.Classification.Style != pattern.DominantStyle;
                bool prefixMismatch = pattern.CommonPrefixes.Count > 0 && observation.Classification.Tokenization.Prefix is not null && !pattern.CommonPrefixes.Contains(observation.Classification.Tokenization.Prefix, StringComparer.OrdinalIgnoreCase);
                if (!styleMismatch && !prefixMismatch)
                    continue;
                double confidence = Math.Min(pattern.Confidence, observation.Classification.Confidence);
                if (confidence < configuration.MinimumViolationConfidence || IsSuppressed(observation, suppressions))
                    continue;
                violations.Add(new NamingViolation
                {
                    Id = $"{observation.Id}:{pattern.Id}",
                    ObservationId = observation.Id,
                    PatternId = pattern.Id,
                    Name = observation.Name,
                    ExpectedStyle = pattern.DominantStyle,
                    ActualStyle = observation.Classification.Style,
                    Severity = confidence >= 0.9 ? NamingViolationSeverity.Warning : NamingViolationSeverity.Information,
                    Confidence = confidence,
                    Message = $"'{observation.Name}' does not match the {pattern.DominantStyle} pattern for {pattern.Scope.SymbolKind} names.",
                    SuggestedName = SuggestName(observation, pattern),
                    Location = observation.Location,
                    Project = observation.Symbol?.Project
                });
            }
        }
        return violations;
    }

    /// <summary>Builds aggregate statistics for observations and violations.</summary>
    public NamingStatistics BuildStatistics(IReadOnlyList<NamingObservation> observations, IReadOnlyList<NamingViolation> violations, int fileCount = 0, int parsedFileCount = 0)
    {
        IEnumerable<NamingObservation> included = observations.Where(item => !item.IsExcluded);
        return new NamingStatistics
        {
            FileCount = fileCount,
            ParsedFileCount = parsedFileCount,
            IdentifierCount = observations.Count,
            DeclarationCount = observations.Count(item => item.Role == NamingNameRole.Declaration || item.Role == NamingNameRole.XamlIdentifier || item.Role == NamingNameRole.ResourceKey),
            ReferenceCount = observations.Count(item => item.Role == NamingNameRole.Reference || item.Role == NamingNameRole.BindingPath || item.Role == NamingNameRole.EventHandler),
            ViolationCount = violations.Count,
            ExcludedCount = observations.Count(item => item.IsExcluded),
            BySymbolKind = included.GroupBy(item => item.Kind).ToDictionary(group => group.Key, group => group.Count()),
            ByRole = included.GroupBy(item => item.Role).ToDictionary(group => group.Key, group => group.Count()),
            ByStyle = included.GroupBy(item => item.Classification.Style).ToDictionary(group => group.Key, group => group.Count()),
            CommonTokens = CountTokens(included),
            CommonPrefixes = CountValues(included.Select(item => item.Classification.Tokenization.Prefix)),
            CommonSuffixes = CountValues(included.Select(item => item.Classification.Tokenization.Suffix))
        };
    }

    private static NamingPattern CreatePattern(NamingSymbolKind kind, NamingSourceLanguage language, string? projectName, IReadOnlyList<NamingObservation> observations, NamingAnalysisConfiguration configuration)
    {
        IGrouping<NamingStyle, NamingObservation>? dominant = observations.GroupBy(item => item.Classification.Style).OrderByDescending(group => group.Count()).FirstOrDefault();
        int count = dominant?.Count() ?? 0;
        double dominance = observations.Count == 0 ? 0 : (double)count / observations.Count;
        return new NamingPattern
        {
            Id = $"{projectName ?? "solution"}:{kind}:{language}",
            Scope = new NamingPatternScope { SymbolKind = kind, Language = language, ProjectName = projectName },
            DominantStyle = dominant?.Key ?? NamingStyle.Unknown,
            CommonPrefixes = CountValues(observations.Select(item => item.Classification.Tokenization.Prefix)).Where(item => item.Value >= Math.Max(2, count / 2)).Select(item => item.Key).ToArray(),
            CommonSuffixes = CountValues(observations.Select(item => item.Classification.Tokenization.Suffix)).Where(item => item.Value >= Math.Max(2, count / 2)).Select(item => item.Key).ToArray(),
            ObservationIds = observations.Select(item => item.Id).ToArray(),
            ObservationCount = observations.Count,
            MatchingCount = count,
            NonMatchingCount = observations.Count - count,
            DominancePercentage = dominance,
            Confidence = observations.Count < configuration.MinimumObservationsPerPattern ? 0 : dominance,
            IsMixed = observations.Count >= configuration.MinimumObservationsPerPattern && dominance < configuration.MinimumDominancePercentage,
            HasInsufficientEvidence = observations.Count < configuration.MinimumObservationsPerPattern
        };
    }

    private static Dictionary<string, int> CountTokens(IEnumerable<NamingObservation> observations) => CountValues(observations.SelectMany(item => item.Classification.Tokenization.Tokens));

    private static Dictionary<string, int> CountValues(IEnumerable<string?> values) => values.Where(value => !string.IsNullOrWhiteSpace(value)).GroupBy(value => value!, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

    private static bool IsSuppressed(NamingObservation observation, IReadOnlyList<NamingSuppression>? suppressions) => suppressions?.Any(item =>
        (item.Expires is null || item.Expires > DateTimeOffset.UtcNow) &&
        (string.Equals(item.Match, observation.Name, StringComparison.Ordinal) ||
         string.Equals(item.Match, observation.SourceFile?.Path, StringComparison.Ordinal) ||
         string.Equals(item.Match, observation.Namespace, StringComparison.Ordinal))) == true;

    private static string? SuggestName(NamingObservation observation, NamingPattern pattern)
    {
        if (pattern.DominantStyle == NamingStyle.PascalCase)
            return string.Concat(observation.Classification.Tokenization.Tokens.Select(token => token.Length == 0 ? token : char.ToUpperInvariant(token[0]) + token[1..].ToLowerInvariant()));
        if (pattern.DominantStyle == NamingStyle.CamelCase)
        {
            string pascal = string.Concat(observation.Classification.Tokenization.Tokens.Select(token => token.Length == 0 ? token : char.ToUpperInvariant(token[0]) + token[1..].ToLowerInvariant()));
            return pascal.Length == 0 ? pascal : char.ToLowerInvariant(pascal[0]) + pascal[1..];
        }
        if (pattern.DominantStyle == NamingStyle.SnakeCase)
            return string.Join('_', observation.Classification.Tokenization.Tokens.Select(token => token.ToLowerInvariant()));
        return null;
    }
}
