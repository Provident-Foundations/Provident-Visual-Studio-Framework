using Provident.Foundations.Abstractions;
using Provident.Foundations.Common;
using Provident.Foundations.VisualStudio.IO;
using Provident.Foundations.VisualStudio.Model;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Provides the methods and functions for validting the XAML style keys in a XAML project.
/// </summary>
public sealed class XamlStyleIdValidator : DisposableObjectBase
{
    #region Public Events
    /// <summary>
    /// Occurs to update the status of the process in the UI.
    /// </summary>
    public event EventHandler<string>? StatusUpdate;
    #endregion

    #region Private Member Declarations
    /// <summary>
    /// The project reference.
    /// </summary>
    private SolutionProjectModel? _project;

    /// <summary>
    /// The solution reference.
    /// </summary>
    private VsSolutionModel? _solution;

    #endregion

    #region Constructor /Dispose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlStyleIdValidator"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor. 
    /// </remarks>
    public XamlStyleIdValidator()
    {
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="XamlStyleIdValidator"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        _project = null;
        _solution = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Private Event Methods
    /// <summary>
    /// Raises the event to update the UI status.
    /// </summary>
    /// <param name="status">
    /// A string containing the text to display to the UI.
    /// </param>
    private void UpdateStatus(string status)
    {
        try
        {
            StatusUpdate?.Invoke(this, status);
        }
        catch
        {
            // In case the UI does something silly.
        }
    }
    #endregion

    #region Public Methods / Functions
    /// <summary>
    /// Validates the XAML content within each project in the solution.
    /// </summary>
    /// <param name="xamlSolution">
    /// The <see cref="VsSolutionModel"/> instance containing the projects.
    /// </param>
    /// <returns>
    /// A <see cref="XamlSolutionValidationResult"/> instance containing the result.
    /// </returns>
    public async Task<XamlSolutionValidationResult> ValidateXamlSolutionAsync(VsSolutionModel xamlSolution)
    {
        XamlSolutionValidationResult solutionResult = new XamlSolutionValidationResult();
        UpdateStatus("Finding And Reading XAML files...");
        _solution = xamlSolution;

        // Validate the XAML in each project and add the results to the solution
        // result.
        foreach (SolutionProjectModel project in _solution.Projects)
        {
            XamlProjectValidationResult projectResult = await ValidateXamlProjectAsync(project);
            foreach(var entry in projectResult.Log.Entries)
            {
                entry.ParentReference = project.Project.Name;
            }
            solutionResult.AddProjectResult(project, projectResult);
            await Task.Yield();
        }

        return solutionResult;
    }

    /// <summary>
    /// Validates the XAML content within the specified project.
    /// </summary>
    /// <param name="xamlProject">
    /// The <see cref="SolutionProjectModel"/> instance containing the projects.
    /// </param>
    /// <returns>
    /// A <see cref="XamlProjectValidationResult"/> instance containing the result of the validation.
    /// </returns>
    public async Task<XamlProjectValidationResult> ValidateXamlProjectAsync(SolutionProjectModel xamlProject)
    {
        XamlProjectValidationResult result = new XamlProjectValidationResult();

        UpdateStatus("Finding And Reading XAML files...");
        _project = xamlProject;

        bool success = await LoadXamlFilesAsync(result);
        if (success)
        {
            UpdateStatus("Starting XAML validation...");
            await PerformXamlValidationAsync(result);
        }

        return result;
    }
    #endregion

    #region Private Methods / Functions
    /// <summary>
    /// Loads the XAML file content into memory for each XAML file in the project.
    /// </summary>
    /// <param name="result">
    /// A <see cref="XamlProjectValidationResult"/> containing the result of the operation.
    /// </param>
    /// <returns>
    /// <b>true</b> if the files and their content are loaded successfully;
    /// otherwise, returns <b>false</b>.
    /// </returns>
    private async Task<bool> LoadXamlFilesAsync(XamlProjectValidationResult result)
    {
        bool success = false;

        if (_project == null)
        {
            result.Log.AddFailure("Project is not set.");
        }
        else
        {
            // Locate the list of files to be read.
            using OperationResult<List<string>> fileFindResult = await XamlIoOperations.FindXamlFilesAsync(_project.Project.FilePath);
            if (!fileFindResult.Success)
            {
                result.Log.AddFailure(fileFindResult);
            }
            else if (fileFindResult.DataContent is not null)
            {
                // Load the list of XAML files into memory.
                int fileCount = fileFindResult.DataContent.Count;
                result.Log.AddInfo($"Found: {fileCount} XAML file(s) to check.");
                UpdateStatus($"Found: {fileCount} XAML file(s) to check.");

                // Load all the data files and parse them into XDocument objects.
                using OperationResult<Dictionary<string, XamlFile>> xamlFiles = await XamlIoOperations.LoadXamlFilesAsync(fileFindResult.DataContent);
                if (!xamlFiles.Success)
                {
                    result.Log.AddFailure(xamlFiles);
                    xamlFiles.Dispose();
                }
                else
                {
                    result.SetXamlFileList(xamlFiles.DataContent);
                    success = true;
                }
            }
            fileFindResult.Dispose();
        }
        return success;
    }

    /// <summary>
    /// Performs validation against the specified loaded XAML files.
    /// </summary>
    /// <returns>
    /// The number of missing resources and circular dependencies found.
    /// </returns>
    private async Task PerformXamlValidationAsync(XamlProjectValidationResult result)
    {
        result.Log.AddInfo("Building Resource Graph...");

        try
        {
            using XamlStyleValidator comparer = new();
            comparer.StatusUpdate += HandleStatusUpdate;

            XamlStyleIdValidationReport report = await comparer.AnalyzeAsync(result.XamlFiles);
            result.SetValidationReport(report);

            if (report.Duplicates is { Count: > 0 } duplicates)
            {
                foreach (XamlDuplicateResource duplicate in duplicates)
                {
                    result.Log.AddFailure($"Duplicate resource '{duplicate.Key}'.  The original is defined in '{duplicate.ExistingFile}'.",
                        duplicate.DuplicateFile);
                }
            }

            if (report.Missing is { Count: > 0 } missingResources)
            {
                foreach (XamlMissingResource missing in missingResources)
                {
                    result.Log.AddFailure($"Missing resource '{missing.Key}' referenced in '{missing.File}' on line {missing.Line}.",
                        missing.File);
                }
            }

            if (report.CircularDependencies is { Count: > 0 } circularDependencies)
            {
                foreach (string dependency in circularDependencies)
                {
                    result.Log.AddFailure($"Circular resource dependency found: {dependency}.");
                }
            }

            comparer.StatusUpdate -= HandleStatusUpdate;
        }
        finally
        {
        }
    }

    /// <summary>
    /// Handles the event from the XML style validator instance to update the UI.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status"></param>
    private void HandleStatusUpdate(object? sender, string status)
    {
        UpdateStatus(status);
    }
    #endregion
}
