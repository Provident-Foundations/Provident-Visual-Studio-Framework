using Provident.Foundations.Abstractions;
using Provident.Foundations.VisualStudio.Model;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Contains the result of a Solution analysis of the contained project and thier XAML files.
/// </summary>
public sealed class XamlSolutionValidationResult : DisposableObjectBase
{
    #region Public Properties
    /// <summary>
    /// Gets a reference to a combined list of results from all projects in the solution.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="XamlProjectValidationResult"/> instances.
    /// </value>
    public List<XamlProjectValidationResult> CombinedResultsList
    {
        get
        {
            List<XamlProjectValidationResult> combinedResults = new();
            foreach(SolutionProjectModel model in ProjectResults.Keys)
            {
                combinedResults.AddRange(ProjectResults[model]);
            }
            return combinedResults;
        }
    }

    /// <summary>
    /// Gets the reference to the list of results for the projects in the solution.
    /// </summary>
    /// <value>
    /// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="SolutionProjectModel"/> instances
    /// as the keys, and the related <see cref="XamlProjectValidationResult"/> instances.
    /// </value>
    public Dictionary<SolutionProjectModel, XamlProjectValidationResult> ProjectResults { get; private set; } = [];
    #endregion

    #region Public Methods / Functions
    /// <summary>
    /// Adds a validation result for a project to the current list.
    /// </summary>
    /// <param name="project">
    /// The reference to the <see cref="SolutionProjectModel"/> project for which the validation result is being added.
    /// </param>
    /// <param name="result">
    /// The reference to the <see cref="XamlProjectValidationResult"/> validation result to add.
    /// </param>
    public void AddProjectResult(SolutionProjectModel project, XamlProjectValidationResult result)
    {
        if (!ProjectResults.ContainsKey(project))
        {
            ProjectResults.Add(project, result);
        }
        else
        {
            ProjectResults[project] = result;
        }
    }
    #endregion
}
