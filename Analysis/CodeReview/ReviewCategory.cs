namespace Provident.Foundations.VisualStudio.Analysis.CodeReview;

/// <summary>
/// Lists the types of categories supported for Code Review operations.
/// </summary>
public enum ReviewCategory
{
    /// <summary>
    /// Indicates the naming of code elements is to be reviewed.
    /// </summary>
    Naming,
    /// <summary>
    /// Indicates the usage of nullability is to be reviewed.
    /// </summary>
    Nullability,

    /// <summary>
    /// Indicates the usage of database operations is to be reviewed.
    /// </summary>
    Database,

    /// <summary>
    /// Indicates an architectural review.
    /// </summary>
    Architecture,

    /// <summary>
    /// Indicates the error handling code is to be reviewed.
    /// </summary>
    ErrorHandling,

    /// <summary>
    /// Indicates the quality of the code is to be reviewed.
    /// </summary>
    CodeQuality,

    /// <summary>
    /// Indicates the testing of the code is to be reviewed.
    /// </summary>
    Testing,

    /// <summary>
    /// Indicates the completeness of the code is to be reviewed.
    /// </summary>  
    Completeness,

    /// <summary>
    /// Indicates the usage of dependency injection is to be reviewed.
    /// </summary>
    DependencyInjection,

    /// <summary>
    /// Indicates the control flow of the code is to be reviewed.
    /// </summary>
    ControlFlow,

    /// <summary>
    /// Indicates code reuse is to be reviewed.
    /// </summary>
    Reuse

}
