using Provident.Foundations.Common;

namespace Provident.Foundations.VisualStudio.IO;

/// <summary>
/// Provides file-system operations used by the validator.
/// </summary>
public static class XamlIoOperations
{
    /// <summary>
    /// Validates a root folder path.
    /// </summary>
    /// <param name="rootFolder">The folder path, or <see langword="null"/> for the current directory.</param>
    /// <returns>The validation result and normalized folder path.</returns>
    public static OperationResult<string> ValidateRootFolder(string? rootFolder)
    {
        OperationResult<string> result = new();
        rootFolder = string.IsNullOrEmpty(rootFolder) ? Directory.GetCurrentDirectory() : rootFolder;

        if (!Directory.Exists(rootFolder))
        {
            result.SetFailureMessage($"Directory not found: {rootFolder}");
        }
        else
        {
            result.Success = true;
            result.DataContent = rootFolder;
        }

        return result;
    }

    /// <summary>
    /// Finds XAML files beneath a root folder.
    /// </summary>
    /// <param name="rootFolder">The folder to search.</param>
    /// <returns>A sorted list of XAML file paths.</returns>
    public static async Task<OperationResult<List<string>>> FindXamlFilesAsync(string rootFolder)
    {
        OperationResult<List<string>> result = new();

        await Task.Yield();
        try
        {
            List<string> files = Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(rootFolder), "*", SearchOption.AllDirectories)
                .Where(fileName => string.Equals(Path.GetExtension(fileName), ".xaml", StringComparison.OrdinalIgnoreCase))
                .Where(fileName => !IsBuildOutputPath(fileName))
                .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            result.Success = true;
            result.DataContent = files;
        }
        catch (Exception exception)
        {
            result.SetFailureMessage("Could not locate the list of XAML files.");
            result.AddException(exception);
        }

        return result;
    }

    /// <summary>
    /// Loads a collection of XAML files.
    /// </summary>
    /// <param name="fileNames">The paths to load.</param>
    /// <returns>The loaded files, or a failure result.</returns>
    public static async Task<OperationResult<Dictionary<string, XamlFile>>> LoadXamlFilesAsync(IReadOnlyList<string> fileNames)
    {
        OperationResult<Dictionary<string, XamlFile>> result = new();
        Dictionary<string, XamlFile> xamlFiles = new();
        bool loadedSuccessfully = true;

        foreach (string fileName in fileNames)
        {
            result.AddMessage($"Reading File: '{fileName}'.");
            XamlFile file = new(fileName);
            using OperationResult fileResult = await file.LoadAsync().ConfigureAwait(false);

            if (!fileResult.Success)
            {
                loadedSuccessfully = false;
                file.Dispose();
                result.SetFailureMessage($"Could not read the file: '{fileName}'");
                result.AddExceptions(fileResult.Exceptions);
                break;
            }

            xamlFiles.Add(fileName, file);
        }

        if (loadedSuccessfully)
        {
            result.Success = true;
            result.DataContent = xamlFiles;
        }
        else
        {
            foreach (XamlFile file in xamlFiles.Values)
            {
                file.Dispose();
            }
        }

        return result;
    }

    private static bool IsBuildOutputPath(string fileName)
    {
        DirectoryInfo? directory = Directory.GetParent(fileName);
        while (directory is not null)
        {
            if (directory.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                directory.Name.Equals("obj", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            directory = directory.Parent;
        }

        return false;
    }
}