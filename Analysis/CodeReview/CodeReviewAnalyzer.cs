using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Provident.Foundations.Abstractions;
using Provident.Foundations.VisualStudio.Model;
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
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the specified directory does not exist.
    /// </exception>
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
                        if (expression.IsKind(SyntaxKind.CoalesceExpression) && expression.Right.ToString() is
                            "string.Empty" or "String.Empty" or "\"\"" or "0" or "false" or "Guid.Empty" or "DateTime.MinValue")
                        {
                            Add(expression, CodeReviewSettings.NullDefaultRule, "Null default", "Check whether this default masks a meaningful missing value.");
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

        CheckAdditionalCSharp(category, root, Add, settings);
    }

    private static void CheckAdditionalCSharp(
        ReviewCategory category,
        SyntaxNode root,
        Action<SyntaxNode, string, string, string> add,
        CodeReviewSettings settings)
    {
        bool IsEnabled(string ruleId) => settings.GetRule(ruleId).Enabled;

        if (category == ReviewCategory.CodeQuality)
        {
            if (IsEnabled(CodeReviewSettings.AsyncVoidRule))
            {
                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    bool eventHandler = method.ParameterList.Parameters.Count == 2 &&
                        method.ParameterList.Parameters[1].Type?.ToString().EndsWith("EventArgs", StringComparison.Ordinal) == true;
                    if (method.Modifiers.Any(SyntaxKind.AsyncKeyword) && method.ReturnType is PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.VoidKeyword } && !eventHandler)
                    {
                        add(method, CodeReviewSettings.AsyncVoidRule, "Async void", "This async method returns void; return Task so callers can await it and observe exceptions.");
                    }
                }
            }

            if (IsEnabled(CodeReviewSettings.AsyncWithoutAwaitRule))
            {
                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                             .Where(method => method.Modifiers.Any(SyntaxKind.AsyncKeyword)))
                {
                    IEnumerable<SyntaxNode> body = method.DescendantNodes(child => child is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax));
                    bool awaits = body.OfType<AwaitExpressionSyntax>().Any() ||
                        body.OfType<CommonForEachStatementSyntax>().Any(loop => loop.AwaitKeyword != default) ||
                        body.OfType<UsingStatementSyntax>().Any(statement => statement.AwaitKeyword != default) ||
                        body.OfType<LocalDeclarationStatementSyntax>().Any(local => local.AwaitKeyword != default);
                    if (!awaits)
                    {
                        add(method, CodeReviewSettings.AsyncWithoutAwaitRule, "Async without await", $"'{method.Identifier.ValueText}' is async but never awaits.");
                    }
                }
            }

            if (IsEnabled(CodeReviewSettings.ThrowExceptionVariableRule))
            {
                foreach (CatchClauseSyntax clause in root.DescendantNodes().OfType<CatchClauseSyntax>())
                {
                    string? name = clause.Declaration?.Identifier.ValueText;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    foreach (ThrowStatementSyntax thrown in clause.Block.DescendantNodes().OfType<ThrowStatementSyntax>()
                                 .Where(thrown => thrown.FirstAncestorOrSelf<CatchClauseSyntax>() == clause &&
                                     thrown.Expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == name))
                    {
                        add(thrown, CodeReviewSettings.ThrowExceptionVariableRule, "Rethrown exception", $"Throwing '{name}' resets the original stack trace; use 'throw;'.");
                    }
                }
            }

            if (IsEnabled(CodeReviewSettings.SyncOverAsyncRule))
            {
                foreach (MemberAccessExpressionSyntax result in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                             .Where(access => access.Name.Identifier.ValueText == "Result" && access.Expression is InvocationExpressionSyntax))
                {
                    add(result, CodeReviewSettings.SyncOverAsyncRule, "Sync over async", "This blocks on an asynchronous result; make the caller async and await it.");
                }

                foreach (InvocationExpressionSyntax call in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (call.ArgumentList.Arguments.Count != 0 || call.Expression is not MemberAccessExpressionSyntax member)
                    {
                        continue;
                    }

                    bool blocksOnAsync = member.Name.Identifier.ValueText == "GetResult" &&
                        member.Expression is InvocationExpressionSyntax awaiter && MethodName(awaiter) == "GetAwaiter";
                    blocksOnAsync |= member.Name.Identifier.ValueText == "Wait" &&
                        member.Expression is InvocationExpressionSyntax;
                    if (blocksOnAsync)
                    {
                        add(call, CodeReviewSettings.SyncOverAsyncRule, "Sync over async", "This blocks on asynchronous work; make the caller async and await it.");
                    }
                }
            }

            if (IsEnabled(CodeReviewSettings.DirectClockRule))
            {
                foreach (MemberAccessExpressionSyntax access in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                             .Where(access => access.ToString() is "DateTime.Now" or "DateTime.UtcNow" or "DateTime.Today" or
                                 "DateTimeOffset.Now" or "DateTimeOffset.UtcNow"))
                {
                    add(access, CodeReviewSettings.DirectClockRule, "Direct clock access", "Read time through an injected TimeProvider so time-dependent behavior can be tested.");
                }
            }

            if (IsEnabled(CodeReviewSettings.InterpolatedLogRule))
            {
                foreach (InvocationExpressionSyntax call in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                             .Where(call => IsLoggingMethod(MethodName(call))))
                {
                    if (call.ArgumentList.Arguments.Any(argument => argument.Expression is InterpolatedStringExpressionSyntax))
                    {
                        add(call, CodeReviewSettings.InterpolatedLogRule, "Interpolated log message", "Use a constant log template with named placeholders and pass values as arguments.");
                    }
                }
            }

            if (IsEnabled(CodeReviewSettings.UnbracedLoopRule))
            {
                foreach (SyntaxNode loop in root.DescendantNodes().Where(node => node is CommonForEachStatementSyntax or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax))
                {
                    StatementSyntax body = loop switch
                    {
                        CommonForEachStatementSyntax each => each.Statement,
                        ForStatementSyntax forLoop => forLoop.Statement,
                        WhileStatementSyntax whileLoop => whileLoop.Statement,
                        DoStatementSyntax doLoop => doLoop.Statement,
                        _ => throw new InvalidOperationException("Unexpected loop syntax node.")
                    };
                    if (body is not BlockSyntax)
                    {
                        add(loop, CodeReviewSettings.UnbracedLoopRule, "Braces", "Add braces to this loop body.");
                    }
                }
            }
        }

        if (category == ReviewCategory.ErrorHandling)
        {
            foreach (CatchClauseSyntax clause in root.DescendantNodes().OfType<CatchClauseSyntax>())
            {
                string? exceptionName = clause.Declaration?.Identifier.ValueText;
                IEnumerable<InvocationExpressionSyntax> logs = clause.Block.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Where(call => IsLoggingMethod(MethodName(call)));

                if (IsEnabled(CodeReviewSettings.LogExceptionMessageRule) && !string.IsNullOrEmpty(exceptionName))
                {
                    foreach (InvocationExpressionSyntax call in logs.Where(call => call.ArgumentList.Arguments.Any(argument =>
                                 argument.Expression.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>().Any(access =>
                                     access.Name.Identifier.ValueText == "Message" && access.Expression.ToString() == exceptionName))))
                    {
                        add(call, CodeReviewSettings.LogExceptionMessageRule, "Exception logging", $"Log the '{exceptionName}' exception object, not just its message, to preserve the stack trace.");
                    }
                }

                if (IsEnabled(CodeReviewSettings.LogAndRethrowRule) &&
                    logs.Any() && clause.Block.DescendantNodes().OfType<ThrowStatementSyntax>().Any(thrown => thrown.FirstAncestorOrSelf<CatchClauseSyntax>() == clause))
                {
                    add(clause, CodeReviewSettings.LogAndRethrowRule, "Duplicate exception logging", "This catch logs and rethrows; log once at the layer that handles the failure.");
                }
            }
        }

        if (category == ReviewCategory.Nullability && IsEnabled(CodeReviewSettings.NullSuppressionRule))
        {
            foreach (PostfixUnaryExpressionSyntax suppression in root.DescendantNodes().OfType<PostfixUnaryExpressionSyntax>()
                         .Where(expression => expression.IsKind(SyntaxKind.SuppressNullableWarningExpression) &&
                             expression.Operand is not LiteralExpressionSyntax and not DefaultExpressionSyntax))
            {
                add(suppression, CodeReviewSettings.NullSuppressionRule, "Null suppression", "This suppresses a possible null without proving the value is present; handle the null explicitly.");
            }
        }

        if (category == ReviewCategory.Database)
        {
            if (IsEnabled(CodeReviewSettings.UtcFieldLocalTimeRule))
            {
                foreach (AssignmentExpressionSyntax assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                {
                    string target = assignment.Left switch
                    {
                        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                        _ => string.Empty
                    };
                    string value = assignment.Right.ToString();
                    if (target.EndsWith("Utc", StringComparison.OrdinalIgnoreCase) &&
                        (value.Contains("DateTime.Now", StringComparison.Ordinal) || value.Contains("DateTimeOffset.Now", StringComparison.Ordinal) ||
                         value.Contains("ToLocalTime(", StringComparison.Ordinal) || value.Contains("TimeZoneInfo.Convert", StringComparison.Ordinal)))
                    {
                        add(assignment, CodeReviewSettings.UtcFieldLocalTimeRule, "UTC assignment", $"'{target}' is named as UTC but receives a local or zoned time.");
                    }
                }
            }

            if (IsEnabled(CodeReviewSettings.MultipleDatabaseWritesRule))
            {
                string[] writeMethods = ["ExecuteAsync", "ExecuteNonQueryAsync", "SaveChangesAsync", "ExecuteSqlRawAsync", "ExecuteSqlInterpolatedAsync", "ExecuteUpdateAsync", "ExecuteDeleteAsync"];
                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    IEnumerable<SyntaxNode> body = method.DescendantNodes(child => child is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax));
                    List<InvocationExpressionSyntax> calls = body.OfType<InvocationExpressionSyntax>().ToList();
                    int writes = calls.Count(call => writeMethods.Contains(MethodName(call), StringComparer.Ordinal));
                    bool transaction = calls.Any(call => MethodName(call) is "BeginTransaction" or "BeginTransactionAsync") ||
                        body.OfType<ObjectCreationExpressionSyntax>().Any(created => created.Type.ToString().EndsWith("TransactionScope", StringComparison.Ordinal));
                    if (writes >= 2 && !transaction)
                    {
                        add(method, CodeReviewSettings.MultipleDatabaseWritesRule, "Database transaction", $"This method makes {writes} database writes without a visible transaction.");
                    }
                }
            }
        }

        if (category == ReviewCategory.Testing && IsEnabled(CodeReviewSettings.InlineDataArrayRule))
        {
            foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                         .Where(method => method.ParameterList.Parameters.Count == 1 && method.ParameterList.Parameters[0].Type is ArrayTypeSyntax))
            {
                foreach (AttributeSyntax attribute in method.AttributeLists.SelectMany(list => list.Attributes)
                             .Where(attribute =>
                             {
                                 string name = attribute.Name.ToString().Split('.').Last().Split("::", StringSplitOptions.None).Last();
                                 return (name is "InlineData" or "InlineDataAttribute") &&
                                     attribute.ArgumentList?.Arguments.Count == 1 &&
                                     attribute.ArgumentList.Arguments[0].Expression is ArrayCreationExpressionSyntax or
                                         ImplicitArrayCreationExpressionSyntax or CollectionExpressionSyntax;
                             }))
                {
                    add(attribute, CodeReviewSettings.InlineDataArrayRule, "InlineData array", "InlineData treats this array as the argument list, not as one array argument.");
                }
            }
        }
    }

    private static string? MethodName(InvocationExpressionSyntax invocation) => invocation.Expression switch
    {
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        SimpleNameSyntax name => name.Identifier.ValueText,
        _ => null
    };

    private static bool IsLoggingMethod(string? name) => name is "Log" or "LogTrace" or "LogDebug" or "LogInformation" or "LogWarning" or "LogError" or "LogCritical";

    private static void CheckSql(ReviewCategory category, SourceFile file, List<ReviewFinding> findings, CodeReviewSettings settings)
    {
        if (category != ReviewCategory.Database)
        {
            return;
        }

        CodeReviewRuleSettings ruleSettings = settings.GetRule(CodeReviewSettings.MigrationGuardRule);
        if (ruleSettings.Enabled)
        {
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

        string[] lines = file.Text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string code = lines[index].TrimEnd('\r').Split("--", 2, StringSplitOptions.None)[0];
            int lineNumber = index + 1;
            if (settings.GetRule(CodeReviewSettings.SqlUtcTimestampRule).Enabled &&
                Regex.IsMatch(code, @"\btimestamp\b(?!\s*with\s+time\s+zone)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(500)))
            {
                AddSqlFinding(findings, category, file, lineNumber, CodeReviewSettings.SqlUtcTimestampRule, "UTC timestamp", "Use 'timestamp with time zone' for a timestamp that represents an instant.", settings);
            }

            if (settings.GetRule(CodeReviewSettings.SqlNamedPrimaryKeyRule).Enabled &&
                Regex.IsMatch(code, @"\bconstraint\s+\S+\s+primary\s+key\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(500)))
            {
                AddSqlFinding(findings, category, file, lineNumber, CodeReviewSettings.SqlNamedPrimaryKeyRule, "Redundant primary-key name", "Let PostgreSQL name the primary-key constraint; write a plain PRIMARY KEY.", settings);
            }
        }
    }

    private static void AddSqlFinding(List<ReviewFinding> findings, ReviewCategory category, SourceFile file, int line, string ruleId, string ruleName, string message, CodeReviewSettings settings)
    {
        CodeReviewRuleSettings ruleSettings = settings.GetRule(ruleId);
        findings.Add(new(category, ruleName, message, Path.GetDirectoryName(file.Path), Path.GetFileName(file.Path), line, false, ruleSettings.Severity ?? ReviewSeverity.Warning));
    }

    private static string Coverage(ReviewCategory category) => category switch
    {
        ReviewCategory.Naming => "Only constant naming is checked automatically; review remaining naming rules and project conventions manually.",
        ReviewCategory.Nullability => "Fake null-coalescing defaults and null-forgiving operators are checked; review contracts, schema and mappings manually.",
        ReviewCategory.Database => "Migration guards, UTC timestamp columns, and named primary-key constraints are checked; review schema, indexes and transaction boundaries manually.",
        ReviewCategory.Architecture => "Architecture, layering and downstream behavior require manual review; no automated rules implemented.",
        ReviewCategory.ErrorHandling => "Empty catches, exception-message-only logging, and log-and-rethrow patterns are checked; review remaining failure paths and observability manually.",
        ReviewCategory.CodeQuality => "Unbraced if/loop bodies, async misuse, sync-over-async, interpolated logging and direct clock access are checked; review remaining readability and public surface manually.",
        ReviewCategory.Testing => "InlineData array argument traps are checked; review test behavior, coverage and fixtures manually.",
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
