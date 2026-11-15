namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Represents a source code file to be analyzed.
/// </summary>
/// <param name="Path">
/// A string containing the full path to the source code file.
/// </param>
/// <param name="Text">
/// A string containing the text content of the source code file.
/// </param>
public sealed record SourceFile(string Path, string Text);
