namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Identifies the solution file format.
/// </summary>
public enum SolutionFormat
{
    /// <summary>
    /// Unknown format.
    /// </summary>
    Unknown,

    /// <summary>
    /// Classic Visual Studio solution.
    /// </summary>
    Sln,

    /// <summary>
    /// Modern solution XML format.
    /// </summary>
    Slnx,
}
