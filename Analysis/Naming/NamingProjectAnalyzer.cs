using Provident.Foundations.VisualStudio.Model;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Analyzes the names contained in one project.</summary>
public sealed class NamingProjectAnalyzer
{
    private readonly CSharpNameExtractor _csharpExtractor;
    private readonly XamlNameExtractor _xamlExtractor;
    private readonly NamingPatternAnalyzer _patternAnalyzer;

    /// <summary>Initializes a project analyzer.</summary>
    public NamingProjectAnalyzer(
        CSharpNameExtractor? csharpExtractor = null,
        XamlNameExtractor? xamlExtractor = null,
        NamingPatternAnalyzer? patternAnalyzer = null)
    {
        _csharpExtractor = csharpExtractor ?? new CSharpNameExtractor();
        _xamlExtractor = xamlExtractor ?? new XamlNameExtractor();
        _patternAnalyzer = patternAnalyzer ?? new NamingPatternAnalyzer();
    }

    /// <summary>Analyzes a project and returns its naming result.</summary>
    public async Task<NamingProjectAnalysisResult> AnalyzeAsync(
        ProjectModel project,
        NamingProjectIdentity identity,
        NamingAnalysisConfiguration? configuration = null,
        IReadOnlyList<NamingSuppression>? suppressions = null,
        IProgress<NamingAnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(identity);
        configuration ??= NamingAnalysisConfiguration.Default;
        DateTimeOffset started = DateTimeOffset.UtcNow;
        List<NamingSourceFileIdentity> files = new();
        List<NamingObservation> observations = new();
        List<NamingDiagnostic> diagnostics = new();
        List<CodeFileModel> codeFiles = project.CodeFiles.Where(file =>
            string.Equals(Path.GetExtension(file.Include ?? file.Update), ".cs", StringComparison.OrdinalIgnoreCase))
            .GroupBy(file => ResolvePath(project.FilePath, file.Include ?? file.Update), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToList();
        List<XamlFileModel> xamlFiles = configuration.IncludeXaml
            ? project.XamlFiles.GroupBy(file => ResolvePath(project.FilePath, file.Include ?? file.Update), StringComparer.OrdinalIgnoreCase).Select(group => group.First()).ToList()
            : new List<XamlFileModel>();
        int total = codeFiles.Count + xamlFiles.Count;
        int completed = 0;
        int parsed = 0;

        foreach (CodeFileModel codeFile in codeFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = ResolvePath(project.FilePath, codeFile.Include ?? codeFile.Update);
            NamingSourceFileIdentity sourceFile = CreateFileIdentity(path, identity, NamingSourceLanguage.CSharp, codeFile.Metadata);
            files.Add(sourceFile);
            if (!ShouldInclude(sourceFile, configuration))
            {
                completed++;
                continue;
            }
            try
            {
                string source = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                observations.AddRange(_csharpExtractor.Extract(source, sourceFile, identity, configuration.IncludeReferences, cancellationToken: cancellationToken));
                parsed++;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                diagnostics.Add(new NamingDiagnostic { Code = "NAM001", Message = $"Unable to read C# file '{path}'.", Detail = exception.Message, Severity = NamingDiagnosticSeverity.Warning, ProjectPath = identity.FilePath });
            }
            completed++;
            progress?.Report(new NamingAnalysisProgress { Stage = "Parsing C# files", ProjectName = identity.Name, FilePath = path, CompletedItems = completed, TotalItems = total });
        }

        if (configuration.IncludeXaml)
        {
            foreach (XamlFileModel xamlFile in xamlFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string path = ResolvePath(project.FilePath, xamlFile.Include ?? xamlFile.Update);
                NamingSourceFileIdentity sourceFile = CreateFileIdentity(path, identity, NamingSourceLanguage.Xaml, xamlFile.Metadata);
                files.Add(sourceFile);
                if (!ShouldInclude(sourceFile, configuration))
                {
                    completed++;
                    continue;
                }
                try
                {
                    string source = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                    observations.AddRange(_xamlExtractor.Extract(source, sourceFile, identity, cancellationToken));
                    parsed++;
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or System.Xml.XmlException)
                {
                    diagnostics.Add(new NamingDiagnostic { Code = "NAM002", Message = $"Unable to parse XAML file '{path}'.", Detail = exception.Message, Severity = NamingDiagnosticSeverity.Warning, ProjectPath = identity.FilePath });
                }
                completed++;
                progress?.Report(new NamingAnalysisProgress { Stage = "Parsing XAML files", ProjectName = identity.Name, FilePath = path, CompletedItems = completed, TotalItems = total });
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<NamingPattern> patterns = _patternAnalyzer.InferPatterns(observations, configuration, identity.Name);
        IReadOnlyList<NamingViolation> violations = _patternAnalyzer.FindViolations(observations, patterns, configuration, suppressions);
        NamingStatistics statistics = _patternAnalyzer.BuildStatistics(observations, violations, files.Count, parsed);
        return new NamingProjectAnalysisResult
        {
            Project = identity,
            Target = new NamingAnalysisTarget { Scope = NamingAnalysisScope.Project, Name = identity.Name, Path = identity.FilePath, ProjectPath = identity.FilePath },
            Status = diagnostics.Count > 0 ? NamingAnalysisStatus.Partial : NamingAnalysisStatus.Completed,
            Files = files,
            Observations = observations,
            Patterns = patterns,
            Violations = violations,
            Statistics = statistics,
            Index = BuildIndex(observations),
            Diagnostics = diagnostics,
            Duration = DateTimeOffset.UtcNow - started
        };
    }

    private static NamingAnalysisIndex BuildIndex(IReadOnlyList<NamingObservation> observations)
    {
        return new NamingAnalysisIndex
        {
            ObservationsById = observations.GroupBy(item => item.Id).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal),
            SymbolsByKey = observations.Where(item => item.Symbol is not null).Select(item => item.Symbol!).GroupBy(item => item.Key).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal),
            ObservationsByFile = observations.Where(item => item.SourceFile is not null).GroupBy(item => item.SourceFile!.Path, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => (IReadOnlyList<NamingObservation>)group.ToArray(), StringComparer.OrdinalIgnoreCase),
            ObservationsByName = observations.GroupBy(item => item.NormalizedName, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => (IReadOnlyList<NamingObservation>)group.ToArray(), StringComparer.OrdinalIgnoreCase)
        };
    }

    private static NamingSourceFileIdentity CreateFileIdentity(string path, NamingProjectIdentity project, NamingSourceLanguage language, IReadOnlyDictionary<string, string> metadata) => new()
    {
        Path = path,
        RelativePath = project.FilePath is null ? path : Path.GetRelativePath(Path.GetDirectoryName(project.FilePath) ?? string.Empty, path),
        Name = Path.GetFileName(path),
        Extension = Path.GetExtension(path),
        Language = language,
        Project = project,
        IsGenerated = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part => string.Equals(part, "obj", StringComparison.OrdinalIgnoreCase)) ||
            metadata.Any(item => item.Key.Contains("generated", StringComparison.OrdinalIgnoreCase) && bool.TryParse(item.Value, out bool generated) && generated)
    };

    private static string ResolvePath(string? projectPath, string? include)
    {
        if (string.IsNullOrWhiteSpace(include))
            return string.Empty;
        return Path.IsPathRooted(include) ? include : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory, include));
    }

    private static bool ShouldInclude(NamingSourceFileIdentity file, NamingAnalysisConfiguration configuration) =>
        (configuration.IncludeGeneratedFiles || !file.IsGenerated) && !configuration.ExcludedFilePatterns.Any(pattern => file.Path.Contains(pattern, StringComparison.OrdinalIgnoreCase));
}
