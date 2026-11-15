using Microsoft.Build.Evaluation;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Provident.Foundations.Abstractions;
using Provident.Foundations.VisualStudio.Model;
using System.ComponentModel;
using System.Text.RegularExpressions;


namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Performs the task of performing a source code review of a specified project or solution.
/// </summary>
public sealed class CodeReviewAnalyzer : DisposableObjectBase
{
    #region Public Events
    #endregion

    #region Private Member Declarations
    #endregion

    #region Public Methods / Functions

    #region Processed
    /// <summary>
    /// Reviews the source code in the specified directory for the all categories.
    /// </summary>
    /// <param name="directory">
    /// A string containing the path in which to find the files.
    /// </param>
    /// <param name="progress">
    /// An optional progress reporter to receive updates on the review progress.
    /// </param>
    /// <param name="cancellationToken">
    /// The async <see cref="CancellationToken"/> instance.
    /// </param>
    /// <param name="workspace">
    /// The reference to the <see cref="VsSolutionModel"/> containing the Visual Studio solution content.
    /// </param>
    /// <returns>
    /// A <see cref="ReviewReport"/> instance containing the results of the code review operation.
    /// </returns>
    public Task<ReviewReport> ReviewAllAsync(string directory, IProgress<ReviewProgress>? progress = null, CancellationToken cancellationToken = default, VsSolutionModel? workspace = null)
    {
        var categories = Enum.GetValues<ReviewCategory>();

        return ReviewAsync(directory, categories, progress, cancellationToken, workspace);
    }

    /// <summary>
    /// Reviews the source code in the specified directory for the specified category.
    /// </summary>
    /// <param name="directory">
    /// A string containing the path in which to find the files.
    /// </param>
    /// <param name="category">
    /// A <see cref="ReviewCategory"/> enumerated value indicating the category of review being performed.
    /// </param>
    /// <param name="progress">
    /// An optional progress reporter to receive updates on the review progress.
    /// </param>
    /// <param name="cancellationToken">
    /// The async <see cref="CancellationToken"/> instance.
    /// </param>
    /// <param name="workspace">
    /// The reference to the <see cref="VsSolutionModel"/> containing the Visual Studio solution content.
    /// </param>
    /// <returns>
    /// A <see cref="ReviewReport"/> instance containing the results of the code review operation.
    /// </returns>
    public Task<ReviewReport> ReviewCategoryAsync(string directory, ReviewCategory category, IProgress<ReviewProgress>? progress = null, CancellationToken cancellationToken = default, VsSolutionModel? workspace = null)
    {
        return ReviewAsync(directory, [category], progress, cancellationToken, workspace);
    }
    #endregion

    #region Processing
    /// <summary>
    /// Performs a code review in all of the specified categories.
    /// </summary>
    /// <param name="directory">
    /// A string containing the path in which to find the files.
    /// </param>
    /// <param name="categories">
    /// An <see cref="IReadOnlyList"/> of  <see cref="ReviewCategory"/> enumerated values indicating the categories of review being performed.
    /// </param>
    /// <param name="progress">
    /// An optional progress reporter to receive updates on the review progress.
    /// </param>
    /// <param name="token">
    /// The async <see cref="CancellationToken"/> instance.
    /// </param>
    /// <param name="workspace">
    /// The reference to the <see cref="VsSolutionModel"/> containing the Visual Studio solution content.
    /// </param>
    /// <returns>
    /// A <see cref="ReviewReport"/> instance containing the results of the code review operation.
    /// </returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    private async Task<ReviewReport> ReviewAsync(string directory, IReadOnlyList<ReviewCategory> categories, IProgress<ReviewProgress>? progress, CancellationToken token, VsSolutionModel? workspace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(categories);
        if (categories.Count == 0)
        {
            throw new ArgumentException("At least one review category is required.", nameof(categories));
        }

        foreach (ReviewCategory category in categories)
        {
            if (!Enum.IsDefined(category))
            {
                throw new ArgumentOutOfRangeException(nameof(categories), category, "Unknown review category.");
            }
        }

        string fullDirectory = Path.GetFullPath(directory);
        if (!Directory.Exists(fullDirectory))
        {
            throw new DirectoryNotFoundException($"Review directory not found: {fullDirectory}");
        }

        token.ThrowIfCancellationRequested();
        progress?.Report(new("Loading review rules", 0, categories.Count));
        CodeReviewSettings settings = await CodeReviewSettingsStore.LoadAsync(fullDirectory, token);
        progress?.Report(new("Locating files", 0, categories.Count));

        List<SourceFile> files = [];
        string? repository = FindRepositoryRoot(fullDirectory);
        string settingsPath = CodeReviewSettingsStore.GetConfigurationPath(fullDirectory);
        string scope = GetScopeDescription(fullDirectory, repository, workspace, settingsPath);
        List<ReviewFinding> diagnostics = await LoadWorkspaceScopeAsync(
            fullDirectory,
            repository,
            workspace,
            settingsPath,
            categories,
            files,
            progress,
            token);

        Dictionary<string, SyntaxNode> syntaxRoots = new(StringComparer.OrdinalIgnoreCase);
        foreach (SourceFile file in files)
        {
            token.ThrowIfCancellationRequested();
            if (CodeReviewFileTypeProvider.GetCodeReviewFileType(Path.GetExtension(file.Path)) != CodeReviewFileTypes.CSharpCodeFile)
            {
                continue;
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(file.Text, path: file.Path, cancellationToken: token);
            SyntaxNode root = syntaxTree.GetRoot(token);
            syntaxRoots.Add(file.Path, root);
            CodeReviewRuleSettings syntaxRule = settings.GetRule(CodeReviewSettings.SyntaxDiagnosticsRule);
            if (syntaxRule.Enabled)
            {
                foreach (Diagnostic diagnostic in syntaxTree.GetDiagnostics(token))
                {
                    if (diagnostic.Location.IsInSource && diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
                    {
                        int line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
                        ReviewSeverity defaultSeverity = diagnostic.Severity == DiagnosticSeverity.Error
                            ? ReviewSeverity.Error
                            : ReviewSeverity.Warning;
                        diagnostics.Add(new(
                            categories.Contains(ReviewCategory.CodeQuality) ? ReviewCategory.CodeQuality : categories[0],
                            CodeReviewSettings.SyntaxDiagnosticsRule,
                            $"{diagnostic.Severity} {diagnostic.Id}: {diagnostic.GetMessage()}",
                            Path.GetDirectoryName(file.Path),
                            Path.GetFileName(file.Path),
                            line,
                            false,
                            syntaxRule.Severity ?? defaultSeverity));
                    }
                }
            }
        }

        for (int index = 0; index < categories.Count; index++)
        {
            token.ThrowIfCancellationRequested();
            ReviewCategory category = categories[index];
            progress?.Report(new($"Reviewing {category}", index, categories.Count));

            foreach (SourceFile file in files)
            {
                token.ThrowIfCancellationRequested();
                CodeReviewFileTypes fileType = CodeReviewFileTypeProvider.GetCodeReviewFileType(Path.GetExtension(file.Path));
                switch (fileType)
                {
                    case CodeReviewFileTypes.VBCodeFile:
                        break;
                    case CodeReviewFileTypes.SQLFile:
                        if (category == ReviewCategory.Database)
                        {
                            CheckSql(category, file, diagnostics, settings);
                        }
                        break;
                    case CodeReviewFileTypes.CSharpCodeFile:
                        CheckCSharp(category, file, syntaxRoots[file.Path], diagnostics, token, settings);
                        break;
                    default:
                        diagnostics.Add(new(category, "Manual review", $"Review this file. {Coverage(category)}", file.Path, null, 1, true));
                        break;
                }
            }

            progress?.Report(new($"Finished {category}", index + 1, categories.Count));
        }

        token.ThrowIfCancellationRequested();
        return new(repository, null, scope, categories.ToArray(), diagnostics, DateTimeOffset.UtcNow);
    }
    #endregion

    #endregion

    #region UnProcessed
    /// <summary>
    /// Loads the list of items to be processed.
    /// </summary>
    /// <param name="directory">
    /// A string containing the path in which to find the files.
    /// </param>
    /// <param name="repository">
    /// A string containing the path to the repository root, if present.
    /// </param>
    /// <param name="workspace">
    /// A Visual Studio solution model representing the loaded workspace, if present.
    /// </param>
    /// <param name="categories">
    /// A list of <see cref="ReviewCategory"/> enumerated values indicating the categories of review being performed.
    /// </param>
    /// <param name="files">
    /// A list of <see cref="SourceFile"/> instances to be populated with the files to be reviewed.
    /// </param>
    /// <param name="progress">
    /// An optional progress reporter to receive updates on the review progress.
    /// </param>
    /// <param name="token">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A <see cref="List{T}"/> of <see cref="ReviewFinding"/> instances.
    /// </returns>
    private static async Task<List<ReviewFinding>> LoadWorkspaceScopeAsync(
        string directory,
        string? repository,
        VsSolutionModel? workspace,
        string settingsPath,
        IReadOnlyList<ReviewCategory> categories,
        List<SourceFile> files,
        IProgress<ReviewProgress>? progress,
        CancellationToken token)
    {
        List<ReviewFinding> diagnostics = [];
        diagnostics.Add(new(categories[0], "Scope", GetScopeDescription(directory, repository, workspace, settingsPath), null, null, null, true));

        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
        Stack<string> pending = new();
        pending.Push(directory);
        if (repository is not null)
        {
            pending.Push(repository);
        }

        if (workspace is not null)
        {
            AddProjectDirectoriesToList(workspace, categories, diagnostics, pending, paths, token);
        }

        HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
        EnumerationOptions options = new()
        {
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        while (pending.TryPop(out string? current))
        {
            token.ThrowIfCancellationRequested();
            ProcessPendingFolder(visited, options, paths, pending, current, categories[0], diagnostics, token);
        }

        foreach (string path in paths.Order(StringComparer.OrdinalIgnoreCase))
        {
            token.ThrowIfCancellationRequested();
            progress?.Report(new($"Reading {path}", 0, categories.Count));
            if (!File.Exists(path))
            {
                diagnostics.Add(new(categories[0], "Scope", "Source file is missing or unavailable and was not reviewed.", path, null, null, true));
                continue;
            }

            bool hasStaticChecks = path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase);
            try
            {
                string text = hasStaticChecks ? await File.ReadAllTextAsync(path, token) : string.Empty;
                files.Add(new(path, text));
                if (!hasStaticChecks)
                {
                    diagnostics.Add(new(categories[0], "Scope", "Visual Basic requires manual review; no automated Visual Basic checks are implemented.", path, null, 1, true));
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                diagnostics.Add(new(categories[0], "Scope", $"Source file could not be read and was not reviewed: {exception.Message}", path, null, null, true));
            }
        }

        if (files.Count == 0)
        {
            diagnostics.Add(new(categories[0], "Scope", "No C#, Visual Basic or SQL migration files were found in the selected scope.", null, null, null, true));
        }

        return diagnostics;
    }

    private static string GetScopeDescription(string directory, string? repository, VsSolutionModel? workspace, string settingsPath)
    {
        string repositoryScope = repository is null
            ? "No Git repository was detected."
            : $"Repository: {repository}.";
        string workspaceScope = workspace is null
            ? "No solution is loaded; only files on disk are reviewed."
            : "Loaded project directories and explicitly listed source items are included.";

        return $"{repositoryScope} Reviewing C#, Visual Basic, and SQL migration files under {directory} and its containing repository if present. {workspaceScope} Rules are loaded from {settingsPath} when that file exists; otherwise built-in defaults are used. Files are reviewed regardless of Git status. SQL migrations are reviewed only for Database. Git metadata and linked directories are excluded from traversal. Unsaved editor contents and deleted files are not reviewed; Visual Basic requires manual review.";
    }

    private static bool IsReviewableFile(string path)
    {
        string extension = Path.GetExtension(path);
        if (extension.Equals(".cs", StringComparison.OrdinalIgnoreCase) || extension.Equals(".vb", StringComparison.OrdinalIgnoreCase))
            return true;
        if (!extension.Equals(".sql", StringComparison.OrdinalIgnoreCase))
            return false;

        if (Regex.IsMatch(Path.GetFileNameWithoutExtension(path), @"(?<![a-z])migrations?(?![a-z])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            return true;
        for (DirectoryInfo? directory = Directory.GetParent(path); directory is not null; directory = directory.Parent)
            if (directory.Name.Equals("migration", StringComparison.OrdinalIgnoreCase) ||
                directory.Name.Equals("migrations", StringComparison.OrdinalIgnoreCase) ||
                directory.Name.Equals("migrate", StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static bool IsExcludedDirectory(string name) => name.Equals(".git", StringComparison.OrdinalIgnoreCase);

    private static void CheckCSharp(ReviewCategory category, SourceFile file, SyntaxNode root, List<ReviewFinding> findings, CancellationToken token, CodeReviewSettings settings)
    {
        void Add(SyntaxNode node, string ruleId, string ruleName, string message)
        {
            CodeReviewRuleSettings ruleSettings = settings.GetRule(ruleId);
            if (!ruleSettings.Enabled)
            {
                return;
            }

            int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            findings.Add(new(
                category,
                ruleName,
                message,
                Path.GetDirectoryName(file.Path),
                Path.GetFileName(file.Path),
                line,
                false,
                ruleSettings.Severity ?? ReviewSeverity.Warning));
        }

        switch (category)
        {
            case ReviewCategory.Naming:
                CodeReviewRuleSettings namingRule = settings.GetRule(CodeReviewSettings.ConstantNamingRule);
                if (namingRule.Enabled)
                {
                    foreach (FieldDeclarationSyntax field in root.DescendantNodes().OfType<FieldDeclarationSyntax>().Where(field => field.Modifiers.Any(SyntaxKind.ConstKeyword)))
                    {
                        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                        {
                            if (!Regex.IsMatch(variable.Identifier.ValueText, namingRule.Pattern!,                            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(500)))
                            {
                                Add(variable, CodeReviewSettings.ConstantNamingRule, "Constant naming", "The constant name does not match the configured naming pattern.");
                            }
                        }
                    }
                }
                break;
            case ReviewCategory.Nullability:
                if (settings.GetRule(CodeReviewSettings.NullDefaultRule).Enabled)
                {
                    foreach (BinaryExpressionSyntax expression in root.DescendantNodes().OfType<BinaryExpressionSyntax>())
                    {
                        if (expression.IsKind(SyntaxKind.CoalesceExpression) && expression.Right.ToString() == "string.Empty")
                        {
                            Add(expression, CodeReviewSettings.NullDefaultRule, "Null default", "Check whether the empty string masks a meaningful null.");
                        }
                    }
                }
                break;
            case ReviewCategory.ErrorHandling:
                if (settings.GetRule(CodeReviewSettings.EmptyCatchRule).Enabled)
                {
                    foreach (CatchClauseSyntax clause in root.DescendantNodes().OfType<CatchClauseSyntax>().Where(clause => clause.Block.Statements.Count == 0))
                    {
                        Add(clause, CodeReviewSettings.EmptyCatchRule, "Swallowed exception", "An empty catch hides the failure; handle or report it.");
                    }
                }
                break;
            case ReviewCategory.CodeQuality:
                if (settings.GetRule(CodeReviewSettings.UnbracedIfRule).Enabled)
                {
                    foreach (IfStatementSyntax statement in root.DescendantNodes().OfType<IfStatementSyntax>())
                    {
                        if (statement.Statement is not BlockSyntax)
                        {
                            Add(statement, CodeReviewSettings.UnbracedIfRule, "Braces", "Add braces to the if body.");
                        }
                    }
                }
                break;
            case ReviewCategory.Completeness:
                CodeReviewRuleSettings deferredWorkRule = settings.GetRule(CodeReviewSettings.DeferredWorkRule);
                if (deferredWorkRule.Enabled)
                {
                    var source = root.SyntaxTree.GetText(token);
                    foreach (SyntaxTrivia trivia in root.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)))
                    {
                        MatchCollection matches = Regex.Matches(
                            trivia.ToString(),
                            deferredWorkRule.Pattern!,
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking,
                            TimeSpan.FromMilliseconds(500));
                        foreach (Match match in matches)
                        {
                            int line = source.Lines.GetLineFromPosition(trivia.SpanStart + match.Index).LineNumber + 1;
                            findings.Add(new(
                                category,
                                "Deferred work",
                                "Verify that this deferred work is tracked and intentional.",
                                Path.GetDirectoryName(file.Path),
                                Path.GetFileName(file.Path),
                                line,
                                false,
                                deferredWorkRule.Severity ?? ReviewSeverity.Warning));
                        }
                    }
                }
                break;
        }
    }

    private static void CheckSql(ReviewCategory category, SourceFile file, List<ReviewFinding> findings, CodeReviewSettings settings)
    {
        if (category != ReviewCategory.Database)
        {
            return;
        }

        CodeReviewRuleSettings ruleSettings = settings.GetRule(CodeReviewSettings.MigrationGuardRule);
        if (!ruleSettings.Enabled)
        {
            return;
        }

        MatchCollection matches = Regex.Matches(
            file.Text,
            ruleSettings.Pattern!,
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking,
            TimeSpan.FromMilliseconds(500));
        foreach (Match match in matches)
        {
            int line = file.Text.AsSpan(0, match.Index).Count('\n') + 1;
            findings.Add(new(
                category,
                "Migration determinism",
                "Check whether this guard hides an unexpected schema state.",
                Path.GetDirectoryName(file.Path),
                Path.GetFileName(file.Path),
                line,
                false,
                ruleSettings.Severity ?? ReviewSeverity.Warning));
        }
    }

    private static string Coverage(ReviewCategory category) => category switch
    {
        ReviewCategory.Naming => "Only constant naming is checked automatically; review remaining naming rules and project conventions manually.",
        ReviewCategory.Nullability => "Only coalescing to string.Empty is checked automatically; review contracts, schema and mappings manually.",
        ReviewCategory.Database => "Only single-line IF EXISTS guards are checked; review schema, indexes, transactions and migrations manually.",
        ReviewCategory.Architecture => "Architecture, layering and downstream behavior require manual review; no automated rules implemented.",
        ReviewCategory.ErrorHandling => "Only empty C# catches are checked; review logging, failure paths and observability manually.",
        ReviewCategory.CodeQuality => "Only unbraced C# if bodies are checked; review readability, dead code and public surface manually.",
        ReviewCategory.Testing => "Test behavior, coverage and fixtures require manual review; no automated rules implemented.",
        ReviewCategory.Completeness => "Only TODO/FIXME comments are checked; verify business rules and removed consumers manually.",
        ReviewCategory.DependencyInjection => "DI lifetime and registration rules are project-specific and require manual review; no automated rules implemented.",
        ReviewCategory.ControlFlow => "Trace guards, catches and loop boundaries manually; no automated rules implemented.",
        ReviewCategory.Reuse => "Search existing implementations and test helpers manually; no automated rules implemented.",
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };
    #endregion

    #region Private Methods / Functions
    /// <summary>
    /// Looks for the parent folder with the ".git" folder, if present.
    /// </summary>
    /// <param name="directory">
    /// A string speciying the directory to start searching from.
    /// </param>
    /// <returns>
    /// A string containing the path to the repository root if found; otherwise, null.
    /// </returns>
    private static string? FindRepositoryRoot(string directory)
    {
        for (DirectoryInfo? current = new(directory); current is not null; current = current.Parent)
        {
            string marker = Path.Combine(current.FullName, ".git");
            if (Directory.Exists(marker) || File.Exists(marker)) return current.FullName;
        }
        return null;
    }

    private static string? AddProjectDirectoryToList(
        SolutionProjectModel entry, 
        IReadOnlyList<ReviewCategory> categories, 
        List<ReviewFinding> diagnostics, 
        Stack<string> pending)
    {
        string? projectDirectory = null;

        if (entry.Project is not ProjectModel project || string.IsNullOrWhiteSpace(project.FilePath))
        {
            diagnostics.Add(new(categories[0], "Scope", $"Project '{entry.Name ?? entry.RelativePath ?? "unknown"}' could not be loaded and was not reviewed.", null, null, null, true));
        }
        else
        {
            try
            {
                projectDirectory = Path.GetDirectoryName(Path.GetFullPath(project.FilePath));
                if (string.IsNullOrWhiteSpace(projectDirectory) || !Directory.Exists(projectDirectory))
                {
                    diagnostics.Add(new(categories[0], "Scope", "Project directory is missing or unavailable and was not reviewed.", project.FilePath, null, null, true));
                    projectDirectory = null;
                }
                else
                {
                    pending.Push(projectDirectory);
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(new(categories[0], "Scope", $"Project directory could not be resolved: {exception.Message}", project.FilePath, null, null, true));
            }
        }
        return projectDirectory;
    }

    private static bool CheckIfFileNameIsValidReviewFile(ProjectItemModel item)
    {
        bool isValid = true;
        // Read the path and file name of the project item.
        string? pathAndFile = item.Include;
        if (!string.IsNullOrEmpty(pathAndFile))
        {
            if (pathAndFile.IndexOfAny(['*', '?', '$', '@', '%']) >= 0)
            {
                string ext = Path.GetExtension(pathAndFile);
                CodeReviewFileTypes fileType = CodeReviewFileTypeProvider.GetCodeReviewFileType(ext);

                if (fileType == CodeReviewFileTypes.CSharpCodeFile ||
                    fileType == CodeReviewFileTypes.VBCodeFile ||
                    fileType == CodeReviewFileTypes.SQLFile)
                {
                    isValid = false;
                }
            }
        }
        return isValid;
    }

    private static void AddProjectDirectoriesToList(
        VsSolutionModel workspace,
        IReadOnlyList<ReviewCategory> categories,
        List<ReviewFinding> diagnostics,
        Stack<string> pending,
        HashSet<string> paths,
        CancellationToken token)
    {
        foreach (SolutionProjectModel entry in workspace.Projects)
        {
            token.ThrowIfCancellationRequested();
            string? projectDirectory = AddProjectDirectoryToList(entry, categories, diagnostics, pending);
            if (entry.Project is not ProjectModel project)
            {
                continue;
            }

            IEnumerable<ProjectItemModel> items = project.CodeFiles.Cast<ProjectItemModel>().Concat(project.XamlFiles);
            foreach (ProjectItemModel item in items)
            {
                token.ThrowIfCancellationRequested();
                string? include = item.Include;
                if (string.IsNullOrWhiteSpace(include) || !CheckIfFileNameIsValidReviewFile(item))
                {
                    diagnostics.Add(new(
                        categories[0],
                        "Scope",
                        $"Project item '{include}' could not be resolved; its review eligibility could not be determined.",
                        project.FilePath,
                        null,
                        null,
                        true));
                    continue;
                }

                if (projectDirectory is null)
                {
                    continue;
                }

                try
                {
                    string path = Path.GetFullPath(include, projectDirectory);
                    if (!IsReviewableFile(path))
                    {
                        continue;
                    }

                    if (File.Exists(path))
                    {
                        paths.Add(path);
                    }
                    else
                    {
                        diagnostics.Add(new(categories[0], "Scope", "Project source file is missing or unavailable and was not reviewed.", path, null, null, true));
                    }
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new(categories[0], "Scope", $"Project item '{include}' could not be resolved: {exception.Message}", project.FilePath, null, null, true));
                }
            }
        }
    }

    private static void ProcessPendingFolder(
        HashSet<string> visited,
        EnumerationOptions options,
        HashSet<string> paths,
        Stack<string> pending,
        string current,
        ReviewCategory category,
        List<ReviewFinding> diagnostics,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string directory;
        try
        {
            directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(current));
        }
        catch (Exception exception)
        {
            diagnostics.Add(new(category, "Scope", $"Directory path could not be resolved: {exception.Message}", current, null, null, true));
            return;
        }

        if (!visited.Add(directory))
        {
            return;
        }

        try
        {
            foreach (string path in Directory.EnumerateFiles(directory, "*", options))
            {
                token.ThrowIfCancellationRequested();
                if (IsReviewableFile(path))
                {
                    paths.Add(path);
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new(category, "Scope", $"Files in this directory could not all be enumerated: {exception.Message}", directory, null, null, true));
        }

        try
        {
            foreach (string child in Directory.EnumerateDirectories(directory, "*", options))
            {
                token.ThrowIfCancellationRequested();
                if (!IsExcludedDirectory(Path.GetFileName(child)))
                {
                    pending.Push(child);
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new(category, "Scope", $"Subdirectories could not all be enumerated: {exception.Message}", directory, null, null, true));
        }
    }
    #endregion
}
