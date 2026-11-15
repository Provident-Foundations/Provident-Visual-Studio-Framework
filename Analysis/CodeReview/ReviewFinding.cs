namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Represents a finding when performing a code review of a solution.
/// </summary>
public sealed record ReviewFinding
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewFinding"/> class.
    /// </summary>
    /// <param name="category">
    /// A <see cref="ReviewCategory"/> enumerated value indicating the review category.
    /// </param>
    /// <param name="ruleName">
    /// A string specifying or describing the rule.
    /// </param>
    /// <param name="message">
    /// A string containing the validation or error message.
    /// </param>
    public ReviewFinding(ReviewCategory category, string ruleName, string message)
    {
        Category = category;
        RuleName = ruleName;
        Message = message;
        Severity = ReviewSeverity.Warning;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewFinding"/> class.
    /// </summary>
    /// <param name="category">
    /// A <see cref="ReviewCategory"/> enumerated value indicating the review category.
    /// </param>
    /// <param name="ruleName">
    /// A string specifying or describing the rule.
    /// </param>
    /// <param name="message">
    /// A string containing the validation or error message.
    /// </param>
    /// <param name="path">
    /// A string specifying the path that contains the code file.
    /// </param>
    /// <param name="file">
    /// A string containing the name of the code file.
    /// </param>
    /// <param name="line">
    /// An integer specifying the number of the line of code where the erorr began.
    /// </param>
    public ReviewFinding(ReviewCategory category, string ruleName, string message, string? path, string? file, int? line)
    {
        Category = category;
        RuleName = ruleName;
        Message = message;
        Path = path;
        File = file;
        Line = line;
        Severity = ReviewSeverity.Warning;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewFinding"/> class.
    /// </summary>
    /// <param name="category">
    /// A <see cref="ReviewCategory"/> enumerated value indicating the review category.
    /// </param>
    /// <param name="ruleName">
    /// A string specifying or describing the rule.
    /// </param>
    /// <param name="message">
    /// A string containing the validation or error message.
    /// </param>
    /// <param name="path">
    /// A string specifying the path that contains the code file.
    /// </param>
    /// <param name="file">
    /// A string containing the name of the code file.
    /// </param>
    /// <param name="line">
    /// An integer specifying the number of the line of code where the erorr began.
    /// </param>
    /// <param name="isManualReview">
    /// A value indicating whether this is a result of a manual review.
    /// </param>
    public ReviewFinding(ReviewCategory category, string ruleName, string message, string? path, string? file, int? line, bool isManualReview)
    {
        Category = category;
        RuleName = ruleName;
        Message = message;
        Path = path;
        File = file;
        Line = line;
        IsManualReview = isManualReview;
        Severity = isManualReview ? ReviewSeverity.Information : ReviewSeverity.Warning;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewFinding"/> class with an explicit severity.
    /// </summary>
    public ReviewFinding(ReviewCategory category, string ruleName, string message, string? path, string? file, int? line, bool isManualReview, ReviewSeverity severity)
        : this(category, ruleName, message, path, file, line, isManualReview)
    {
        Severity = severity;
    }

    /// <summary>
    /// Gets or sets the review category.
    /// </summary>
    /// <value>
    /// A <see cref="ReviewCategory"/> enumerated value.
    /// </value>
    public ReviewCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the name of the rule.
    /// </summary>
    /// <value>
    /// A string specifying or describing the rule.
    /// </value>
    public string RuleName { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    /// <value>
    /// A string containing the validation or error message.
    /// </value>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the path that contains the code file.
    /// </summary>
    /// <value>
    /// A string specifying the path that contains the code file.
    /// </value>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>
    /// A string containing the name of the code file.
    /// </value>
    public string? File { get; set; }

    /// <summary>
    /// Gets or sets the line number.
    /// </summary>
    /// <value>
    /// An integer specifying the number of the line of code where the error began.
    /// </value>
    public int? Line { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a result of a manual review.
    /// </summary>
    /// <value>
    /// <b>true</b> for a manual review; otherwise, <b>false</b>.
    /// </value>
    public bool IsManualReview { get; set; }

    /// <summary>
    /// Gets or sets the severity of the finding.
    /// </summary>
    public ReviewSeverity Severity { get; set; }
}

