using Provident.Foundations.IO.FileTypes;

namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Provides the static factory methods and functions for determining file types for code review.
/// </summary>
public static class CodeReviewFileTypeProvider
{
    #region Private Member Declarations
    private const string ExtCS = "cs";
    private const string ExtVB = "vb";
    private const string ExtSql = "sql";

    private static FileTypeInfoCollection _systemFileTypes = FileTypeInfoCollection.LoadFileTypes();
    #endregion

    #region Public Static Methods / Functions
    /// <summary>
    /// Gets the file type information for the specified extension.
    /// </summary>
    /// <param name="extension">
    /// A string containing the file extension.
    /// </param>
    /// <returns>
    /// The related <see cref="FileTypeInfo"/> instance, or <b>null</b> if not found.
    /// </returns>
    public static FileTypeInfo? GetFileType(string extension)
    {
        FileTypeInfo? info = null;

        if (!string.IsNullOrEmpty(extension))
        {
            info = _systemFileTypes[extension];
        }
        return info;
    }

    /// <summary>
    /// Gets the type of file for code review purposes.
    /// </summary>
    /// <param name="extension">
    /// A string containing the file extension value.
    /// </param>
    /// <returns>
    /// A <see cref="CodeReviewFileTypes"/> enumerated value.
    /// </returns>
    public static CodeReviewFileTypes GetCodeReviewFileType(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return CodeReviewFileTypes.UnSupported;
        }

        string normalizedExtension = extension.Trim();
        if (normalizedExtension.StartsWith(".", StringComparison.Ordinal))
        {
            normalizedExtension = normalizedExtension[1..];
        }

        return normalizedExtension.ToLowerInvariant() switch
        {
            ExtCS => CodeReviewFileTypes.CSharpCodeFile,
            ExtVB => CodeReviewFileTypes.VBCodeFile,
            ExtSql => CodeReviewFileTypes.SQLFile,
            _ => CodeReviewFileTypes.UnSupported,
        };
    }
    #endregion

}
