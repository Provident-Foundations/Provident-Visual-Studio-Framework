namespace Provident.Foundations.VisualStudio.Analysis.Naming.Analysis;

/// <summary>
/// Lists the items whose naming may be analyzed.
/// </summary>
public enum NamingAnalysisScope
{
    /// <summary>
    /// Indicates a Visual Studio project. 
    /// </summary>
    Project,

    /// <summary>
    /// Indicates a Visual Studio solution.
    /// </summary>
    Solution,

    /// <summary>
    /// Indicates a general set of files.
    /// </summary>
    FileSet
}
