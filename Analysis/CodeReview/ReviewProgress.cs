namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Represents the current progress status while performing a code review.
/// </summary>
/// <param name="Message">
/// A string containing the progress message.
/// </param>
/// <param name="Completed">
/// An integer specifying the number of items completed.
/// </param>
/// <param name="Total">
/// An integer specifying the total number of items to be examined.
/// </param>
public sealed record ReviewProgress(string Message, int Completed, int Total);