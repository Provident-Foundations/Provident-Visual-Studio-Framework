namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Configures whether a review rule runs and the severity assigned to its findings.
/// </summary>
public sealed class CodeReviewRuleSettings
{
    /// <summary>
    /// Gets or sets whether the rule is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the finding severity; null uses the rule's default.
    /// </summary>
    public ReviewSeverity? Severity { get; set; }

    /// <summary>
    /// Gets or sets the rule's regular expression, where supported.
    /// </summary>
    public string? Pattern { get; set; }
}
