using Provident.Foundations.VisualStudio.Model;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Provident.Foundations.VisualStudio.IO;

/// <summary>
/// Loads Visual Studio solutions and their referenced projects.
/// </summary>
public sealed class VisualStudioSolutionLoader
{
    private static readonly Regex ClassicProjectRegex = new(
        "^Project\\(\\\"(?<type>[^\\\"]+)\\\"\\) = \\\"(?<name>[^\\\"]+)\\\", \\\"(?<path>[^\\\"]+)\\\", \\\"(?<guid>[^\\\"]+)\\\"",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private readonly VisualStudioProjectLoader _projectLoader;

    /// <summary>
    /// Initializes a solution loader.
    /// </summary>
    /// <param name="projectLoader">The project loader to use.</param>
    public VisualStudioSolutionLoader(VisualStudioProjectLoader? projectLoader = null)
    {
        _projectLoader = projectLoader ?? new VisualStudioProjectLoader();
    }

    /// <summary>
    /// Loads a solution and all project files that it references.
    /// </summary>
    /// <param name="solutionPath">The path to a `.sln` or `.slnx` file.</param>
    /// <returns>The populated solution model.</returns>
    public VsSolutionModel Load(string solutionPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionPath);
        string fullPath = Path.GetFullPath(solutionPath);
        return Path.GetExtension(fullPath).Equals(".slnx", StringComparison.OrdinalIgnoreCase)
            ? LoadSlnx(fullPath)
            : LoadSln(fullPath);
    }

    /// <summary>
    /// Loads a solution and all project files that it references.
    /// </summary>
    /// <param name="solutionPath">The path to a `.sln` or `.slnx` file.</param>
    /// <returns>The populated solution model.</returns>
    public async Task<VsSolutionModel> LoadAsync(string solutionPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionPath);
        string fullPath = Path.GetFullPath(solutionPath);
        return Path.GetExtension(fullPath).Equals(".slnx", StringComparison.OrdinalIgnoreCase)
            ? await LoadSlnxAsync(fullPath)
            : await LoadSlnAsync(fullPath);
    }

    /// <summary>
    /// Loads a solution as the richer XAML-aware model.
    /// </summary>
    /// <param name="solutionPath">The path to a `.sln` or `.slnx` file.</param>
    /// <returns>The populated XAML solution model.</returns>
    public async Task<VsSolutionModel> LoadSolutionAsync(string solutionPath)
    {
        await Task.Yield();

        VsSolutionModel loaded = Load(solutionPath);
        VsSolutionModel solution = new()
        {
            FilePath = loaded.FilePath,
            Name = loaded.Name,
            Format = loaded.Format,
        };

        solution.Configurations.AddRange(loaded.Configurations);
        solution.Projects.AddRange(loaded.Projects);
        solution.Folders.AddRange(loaded.Folders);
        return solution;
    }

    /// <summary>
    /// Loads a solution as the richer XAML-aware model.
    /// </summary>
    /// <param name="solutionPath">The path to a `.sln` or `.slnx` file.</param>
    /// <returns>The populated XAML solution model.</returns>
    public VsSolutionModel LoadSolution(string solutionPath)
    {
        VsSolutionModel loaded = Load(solutionPath);
        VsSolutionModel solution = new()
        {
            FilePath = loaded.FilePath,
            Name = loaded.Name,
            Format = loaded.Format,
        };

        solution.Configurations.AddRange(loaded.Configurations);
        solution.Projects.AddRange(loaded.Projects);
        solution.Folders.AddRange(loaded.Folders);
        return solution;
    }

    private async Task<VsSolutionModel> LoadSlnAsync(string solutionPath)
    {
        string[] lines = await File.ReadAllLinesAsync(solutionPath);
        VsSolutionModel solution = CreateSolution(solutionPath, SolutionFormat.Sln);
        string solutionDirectory = Path.GetDirectoryName(solutionPath)!;
        Dictionary<string, SolutionProjectModel> projectsByGuid = new(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (string line in lines)
        {
            Match projectMatch = ClassicProjectRegex.Match(line);
            if (projectMatch.Success)
            {
                string projectType = projectMatch.Groups["type"].Value;
                string name = projectMatch.Groups["name"].Value;
                string relativePath = projectMatch
                    .Groups["path"]
                    .Value.Replace('\\', Path.DirectorySeparatorChar);
                string projectGuid = projectMatch.Groups["guid"].Value;
                if (IsSolutionFolder(projectType))
                {
                    solution.Folders.Add(
                        new SolutionFolderModel { Name = name, RelativePath = relativePath }
                    );
                    continue;
                }

                string projectPath = Path.GetFullPath(
                    Path.Combine(solutionDirectory, relativePath)
                );
                ProjectModel? project = File.Exists(projectPath)
                    ? _projectLoader.Load(projectPath)
                    : null;
                SolutionProjectModel entry = new()
                {
                    Name = name,
                    RelativePath = relativePath,
                    ProjectGuid = projectGuid,
                    Project = project,
                };
                solution.Projects.Add(entry);
                projectsByGuid[projectGuid] = entry;
                continue;
            }

            if (line.Contains("SolutionConfigurationPlatforms", StringComparison.Ordinal))
            {
                continue;
            }

            Match configurationMatch = Regex.Match(
                line,
                @"^\s*(?<name>[^|=]+)\|(?<platform>[^=]+)="
            );
            if (configurationMatch.Success)
            {
                solution.Configurations.Add(
                    new SolutionConfigurationModel
                    {
                        Name = configurationMatch.Groups["name"].Value.Trim(),
                        Platform = configurationMatch.Groups["platform"].Value.Trim(),
                        IsActive = true,
                    }
                );
            }
        }

        return solution;
    }
    private VsSolutionModel LoadSln(string solutionPath)
    {
        string[] lines = File.ReadAllLines(solutionPath);
        VsSolutionModel solution = CreateSolution(solutionPath, SolutionFormat.Sln);
        string solutionDirectory = Path.GetDirectoryName(solutionPath)!;
        Dictionary<string, SolutionProjectModel> projectsByGuid = new(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (string line in lines)
        {
            Match projectMatch = ClassicProjectRegex.Match(line);
            if (projectMatch.Success)
            {
                string projectType = projectMatch.Groups["type"].Value;
                string name = projectMatch.Groups["name"].Value;
                string relativePath = projectMatch
                    .Groups["path"]
                    .Value.Replace('\\', Path.DirectorySeparatorChar);
                string projectGuid = projectMatch.Groups["guid"].Value;
                if (IsSolutionFolder(projectType))
                {
                    solution.Folders.Add(
                        new SolutionFolderModel { Name = name, RelativePath = relativePath }
                    );
                    continue;
                }

                string projectPath = Path.GetFullPath(
                    Path.Combine(solutionDirectory, relativePath)
                );
                ProjectModel? project = File.Exists(projectPath)
                    ? _projectLoader.Load(projectPath)
                    : null;
                SolutionProjectModel entry = new()
                {
                    Name = name,
                    RelativePath = relativePath,
                    ProjectGuid = projectGuid,
                    Project = project,
                };
                solution.Projects.Add(entry);
                projectsByGuid[projectGuid] = entry;
                continue;
            }

            if (line.Contains("SolutionConfigurationPlatforms", StringComparison.Ordinal))
            {
                continue;
            }

            Match configurationMatch = Regex.Match(
                line,
                @"^\s*(?<name>[^|=]+)\|(?<platform>[^=]+)="
            );
            if (configurationMatch.Success)
            {
                solution.Configurations.Add(
                    new SolutionConfigurationModel
                    {
                        Name = configurationMatch.Groups["name"].Value.Trim(),
                        Platform = configurationMatch.Groups["platform"].Value.Trim(),
                        IsActive = true,
                    }
                );
            }
        }

        return solution;
    }

    private async Task<VsSolutionModel> LoadSlnxAsync(string solutionPath)
    {
        await Task.Yield();
        return LoadSlnx(solutionPath);
    }

    private VsSolutionModel LoadSlnx(string solutionPath)
    {
        XDocument document = XDocument.Load(solutionPath);
        XElement root =
            document.Root
            ?? throw new InvalidDataException("The solution file has no root element.");
        VsSolutionModel solution = CreateSolution(solutionPath, SolutionFormat.Slnx);
        string directory = Path.GetDirectoryName(solutionPath)!;

        foreach (XElement element in root.Descendants())
        {
            string localName = element.Name.LocalName;
            string? path =
                AttributeValue(element, "path")
                ?? AttributeValue(element, "Path")
                ?? AttributeValue(element, "project");
            if (
                localName.Equals("Project", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(path)
            )
            {
                string relativePath = path.Replace('/', Path.DirectorySeparatorChar);
                string projectPath = Path.GetFullPath(Path.Combine(directory, relativePath));
                ProjectModel? project = File.Exists(projectPath)
                    ? _projectLoader.Load(projectPath)
                    : null;
                solution.Projects.Add(
                    new SolutionProjectModel
                    {
                        Name = Path.GetFileNameWithoutExtension(relativePath),
                        RelativePath = relativePath,
                        Project = project,
                    }
                );
            }
            else if (localName.Equals("Folder", StringComparison.OrdinalIgnoreCase))
            {
                solution.Folders.Add(
                    new SolutionFolderModel
                    {
                        Name = AttributeValue(element, "name") ?? AttributeValue(element, "Name"),
                        RelativePath = path,
                    }
                );
            }
            else if (localName.Equals("Configuration", StringComparison.OrdinalIgnoreCase))
            {
                solution.Configurations.Add(
                    new SolutionConfigurationModel
                    {
                        Name = AttributeValue(element, "name") ?? AttributeValue(element, "Name"),
                        Platform =
                            AttributeValue(element, "platform")
                            ?? AttributeValue(element, "Platform"),
                        IsActive = true,
                    }
                );
            }
        }

        return solution;
    }

    private static VsSolutionModel CreateSolution(string path, SolutionFormat format) =>
        new()
        {
            FilePath = path,
            Name = Path.GetFileNameWithoutExtension(path),
            Format = format,
        };

    private static bool IsSolutionFolder(string projectType) =>
        projectType.Equals(
            "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}",
            StringComparison.OrdinalIgnoreCase
        );

    private static string? AttributeValue(XElement element, string name) =>
        element
            .Attributes()
            .FirstOrDefault(attribute =>
                attribute.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)
            )
            ?.Value;
}
