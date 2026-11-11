using Provident.Foundations.VisualStudio.Model;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Identifies the scope being analyzed.</summary>
public sealed class NamingAnalysisTarget
{
    /// <summary>Gets the target scope.</summary>
    public NamingAnalysisScope Scope { get; init; }
    /// <summary>Gets the display name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the target path.</summary>
    public string? Path { get; init; }
    /// <summary>Gets the solution path, when applicable.</summary>
    public string? SolutionPath { get; init; }
    /// <summary>Gets the project path, when applicable.</summary>
    public string? ProjectPath { get; init; }
}

/// <summary>Identifies a project participating in analysis.</summary>
public sealed class NamingProjectIdentity
{
    /// <summary>Gets the project name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the absolute project path.</summary>
    public string? FilePath { get; init; }
    /// <summary>Gets the project path relative to the solution.</summary>
    public string? RelativePath { get; init; }
    /// <summary>Gets the project identifier.</summary>
    public string? ProjectGuid { get; init; }
    /// <summary>Gets the root namespace.</summary>
    public string? RootNamespace { get; init; }
    /// <summary>Gets the target frameworks.</summary>
    public IReadOnlyList<string> TargetFrameworks { get; init; } = Array.Empty<string>();

    /// <summary>Creates an identity from the existing project models.</summary>
    public static NamingProjectIdentity From(SolutionProjectModel project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return new NamingProjectIdentity
        {
            Name = project.Name ?? project.Project?.Name ?? string.Empty,
            FilePath = project.Project?.FilePath,
            RelativePath = project.RelativePath,
            ProjectGuid = project.ProjectGuid ?? project.Project?.ProjectGuid,
            RootNamespace = project.Project?.Properties.TryGetValue("RootNamespace", out string? root) == true ? root : null,
            TargetFrameworks = project.Project?.Properties.TryGetValue("TargetFramework", out string? framework) == true && framework is not null
                ? new[] { framework }
                : Array.Empty<string>()
        };
    }
}

/// <summary>Identifies a source file participating in analysis.</summary>
public sealed class NamingSourceFileIdentity
{
    /// <summary>Gets the absolute file path.</summary>
    public string Path { get; init; } = string.Empty;
    /// <summary>Gets the project-relative path.</summary>
    public string? RelativePath { get; init; }
    /// <summary>Gets the file name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the file extension.</summary>
    public string Extension { get; init; } = string.Empty;
    /// <summary>Gets the source language.</summary>
    public NamingSourceLanguage Language { get; init; }
    /// <summary>Gets the owning project.</summary>
    public NamingProjectIdentity? Project { get; init; }
    /// <summary>Gets whether the file appears to be generated.</summary>
    public bool IsGenerated { get; init; }
    /// <summary>Gets the content hash, when calculated.</summary>
    public string? ContentHash { get; init; }
}

/// <summary>Describes a source location.</summary>
public sealed class NamingSourceSpan
{
    /// <summary>Gets the source file path.</summary>
    public string FilePath { get; init; } = string.Empty;
    /// <summary>Gets the one-based starting line.</summary>
    public int StartLine { get; init; }
    /// <summary>Gets the one-based starting column.</summary>
    public int StartColumn { get; init; }
    /// <summary>Gets the one-based ending line.</summary>
    public int EndLine { get; init; }
    /// <summary>Gets the one-based ending column.</summary>
    public int EndColumn { get; init; }
    /// <summary>Gets the zero-based character offset.</summary>
    public int CharacterOffset { get; init; }
    /// <summary>Gets the character length.</summary>
    public int CharacterLength { get; init; }
    /// <summary>Gets an optional source excerpt.</summary>
    public string? Text { get; init; }
}

/// <summary>Identifies a resolved source symbol.</summary>
public sealed class NamingSymbolIdentity
{
    /// <summary>Gets the stable Roslyn symbol key or fallback key.</summary>
    public string Key { get; init; } = string.Empty;
    /// <summary>Gets the simple symbol name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the fully qualified name.</summary>
    public string FullyQualifiedName { get; init; } = string.Empty;
    /// <summary>Gets the symbol kind.</summary>
    public NamingSymbolKind Kind { get; init; }
    /// <summary>Gets the containing namespace.</summary>
    public string? Namespace { get; init; }
    /// <summary>Gets the containing symbol key.</summary>
    public string? ContainingSymbolKey { get; init; }
    /// <summary>Gets the owning project.</summary>
    public NamingProjectIdentity? Project { get; init; }
    /// <summary>Gets the accessibility text.</summary>
    public string? Accessibility { get; init; }
    /// <summary>Gets whether the symbol is static.</summary>
    public bool IsStatic { get; init; }
    /// <summary>Gets whether the symbol has multiple declarations.</summary>
    public bool IsPartial { get; init; }
}
