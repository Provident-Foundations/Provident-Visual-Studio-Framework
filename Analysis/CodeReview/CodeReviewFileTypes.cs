namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Lists the type of files that can be code reviewed.
/// </summary>
public enum CodeReviewFileTypes
{
    /// <summary>
    /// Indicates the file type is not supported.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a .cs (C#) code file.
    /// </summary>
    CSharpCodeFile = 1,

    /// <summary>
    /// Indicates a .vb (Visual Basic) code file.
    /// </summary>
    VBCodeFile = 2,

    /// <summary>
    /// Indicates a .sql (SQL) code file.
    /// </summary>
    SQLFile = 3,

    /// <summary>
    /// Indicates that the file type is not supported for code review.
    /// </summary>
    UnSupported = 4
}
