namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Represents and manages the report for a code review, including repository information, scope, categories, findings, and completion timestamp.
/// </summary>
/// <param name="Repository">
/// A string representing the repository being reviewed. This can be null if the repository information is not available.
/// </param>
/// <param name="BaseCommit">
/// A string representing the base commit of the repository being reviewed. This can be null if the base commit information is not available.
/// </param>
/// <param name="Scope">
/// A string specifying the scope of the code review. This is a required field and cannot be null.
/// </param>
/// <param name="Categories">
/// The list of review categories that are part of the code review. This is a required field and cannot be null.
/// </param>
/// <param name="Findings">
/// A list of the findings identified during the code review. This is a required field and cannot be null.
/// </param>
/// <param name="CompletedAt">
/// The <see cref="DateTimeOffset"/> specifying the date and time the review was completed.
/// </param>
public sealed record ReviewReport(string? Repository, string? BaseCommit, string Scope, IReadOnlyList<ReviewCategory> Categories, IReadOnlyList<ReviewFinding> Findings, DateTimeOffset CompletedAt);