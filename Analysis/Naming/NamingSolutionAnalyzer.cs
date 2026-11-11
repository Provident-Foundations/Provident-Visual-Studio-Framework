using Provident.Foundations.VisualStudio.Model;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Analyzes naming patterns across all projects in a solution.</summary>
public sealed class NamingSolutionAnalyzer
{
    private readonly NamingProjectAnalyzer _projectAnalyzer;
    private readonly NamingPatternAnalyzer _patternAnalyzer;

    /// <summary>Initializes a solution analyzer.</summary>
    public NamingSolutionAnalyzer(NamingProjectAnalyzer? projectAnalyzer = null, NamingPatternAnalyzer? patternAnalyzer = null)
    {
        _projectAnalyzer = projectAnalyzer ?? new NamingProjectAnalyzer();
        _patternAnalyzer = patternAnalyzer ?? new NamingPatternAnalyzer();
    }

    /// <summary>Analyzes the projects represented by a solution model.</summary>
    public async Task<NamingSolutionAnalysisResult> AnalyzeAsync(
        VsSolutionModel solution,
        NamingAnalysisConfiguration? configuration = null,
        IReadOnlyList<NamingSuppression>? suppressions = null,
        IProgress<NamingAnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solution);
        configuration ??= NamingAnalysisConfiguration.Default;
        DateTimeOffset started = DateTimeOffset.UtcNow;
        List<NamingSolutionProjectResult> projects = new();
        List<NamingDiagnostic> diagnostics = new();
        List<NamingObservation> observations = new();
        int total = solution.Projects.Count;
        int completed = 0;

        foreach (SolutionProjectModel project in solution.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            NamingProjectIdentity identity = NamingProjectIdentity.From(project);
            if (project.Project is null)
            {
                NamingDiagnostic diagnostic = new() { Code = "NAM101", Message = $"Project '{identity.Name}' was not loaded and could not be analyzed.", Severity = NamingDiagnosticSeverity.Warning, ProjectPath = identity.FilePath };
                projects.Add(new NamingSolutionProjectResult { Project = identity, Status = NamingAnalysisStatus.Failed, Diagnostics = new[] { diagnostic } });
                diagnostics.Add(diagnostic);
                completed++;
                continue;
            }

            try
            {
                NamingProjectAnalysisResult result = await _projectAnalyzer.AnalyzeAsync(project.Project, identity, configuration, suppressions, progress, cancellationToken).ConfigureAwait(false);
                projects.Add(new NamingSolutionProjectResult
                {
                    Project = identity,
                    Dependencies = project.Project.ProjectReferences.Select(reference => reference.Include ?? string.Empty).Where(value => value.Length > 0).ToArray(),
                    Status = result.Status,
                    Result = result,
                    Diagnostics = result.Diagnostics
                });
                observations.AddRange(result.Observations);
                diagnostics.AddRange(result.Diagnostics);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                NamingDiagnostic diagnostic = new() { Code = "NAM102", Message = $"Project '{identity.Name}' could not be analyzed.", Detail = exception.Message, Severity = NamingDiagnosticSeverity.Error, ProjectPath = identity.FilePath };
                projects.Add(new NamingSolutionProjectResult { Project = identity, Status = NamingAnalysisStatus.Failed, Diagnostics = new[] { diagnostic } });
                diagnostics.Add(diagnostic);
            }
            completed++;
            progress?.Report(new NamingAnalysisProgress { Stage = "Analyzing projects", ProjectName = identity.Name, CompletedItems = completed, TotalItems = total });
        }

        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<NamingPattern> patterns = _patternAnalyzer.InferPatterns(observations, configuration);
        IReadOnlyList<NamingViolation> violations = _patternAnalyzer.FindViolations(observations, patterns, configuration, suppressions);
        NamingStatistics statistics = _patternAnalyzer.BuildStatistics(observations, violations, projects.Sum(item => item.Result?.Statistics.FileCount ?? 0), projects.Sum(item => item.Result?.Statistics.ParsedFileCount ?? 0));
        return new NamingSolutionAnalysisResult
        {
            Target = new NamingAnalysisTarget { Scope = NamingAnalysisScope.Solution, Name = solution.Name ?? string.Empty, Path = solution.FilePath, SolutionPath = solution.FilePath },
            Projects = projects,
            Observations = observations,
            Patterns = patterns,
            Violations = violations,
            Statistics = statistics,
            Index = BuildIndex(observations),
            Diagnostics = diagnostics,
            Status = projects.Any(item => item.Status != NamingAnalysisStatus.Completed) ? NamingAnalysisStatus.Partial : NamingAnalysisStatus.Completed,
            Duration = DateTimeOffset.UtcNow - started
        };
    }

    private static NamingAnalysisIndex BuildIndex(IReadOnlyList<NamingObservation> observations) => new()
    {
        ObservationsById = observations.GroupBy(item => item.Id).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal),
        SymbolsByKey = observations.Where(item => item.Symbol is not null).Select(item => item.Symbol!).GroupBy(item => item.Key).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal),
        ObservationsByFile = observations.Where(item => item.SourceFile is not null).GroupBy(item => item.SourceFile!.Path, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => (IReadOnlyList<NamingObservation>)group.ToArray(), StringComparer.OrdinalIgnoreCase),
        ObservationsByName = observations.GroupBy(item => item.NormalizedName, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => (IReadOnlyList<NamingObservation>)group.ToArray(), StringComparer.OrdinalIgnoreCase)
    };
}
