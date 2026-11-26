using System.Security.Principal;

namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a project in a solution.
/// </summary>
public class ProjectModel
{
    /// <summary>
    /// Gets or initializes the project path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets or initializes the project name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the project GUID.
    /// </summary>
    public string? ProjectGuid { get; set; }

    /// <summary>
    /// Gets or initializes the project SDK.
    /// </summary>
    public ProjectSdkModel Sdk { get; set; } = new();

    /// <summary>
    /// Gets the evaluated project properties.
    /// </summary>
    public Dictionary<string, string> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the project property groups.
    /// </summary>
    public List<PropertyGroupModel> PropertyGroups { get; } = [];

    /// <summary>
    /// Gets the project item groups.
    /// </summary>
    public List<ItemGroupModel> ItemGroups { get; } = [];

    /// <summary>
    /// Gets package references.
    /// </summary>
    public List<PackageReferenceModel> PackageReferences { get; } = [];

    /// <summary>
    /// Gets project references.
    /// </summary>
    public List<ProjectReferenceModel> ProjectReferences { get; } = [];

    /// <summary>
    /// Gets build configurations.
    /// </summary>
    public List<ProjectConfigurationModel> Configurations { get; } = [];

    /// <summary>
    /// Gets source files.
    /// </summary>
    public List<CodeFileModel> CodeFiles { get; } = [];

    /// <summary>
    /// Gets XAML files.
    /// </summary>
    public List<XamlFileModel> XamlFiles { get; } = [];

    /// <summary>
    /// Gets packaging metadata.
    /// </summary>
    public WinUiPackagingModel Packaging { get; set; } = new();

    /// <summary>
    /// Attempts to get the root namepsace of the project.
    /// </summary>
    /// <returns>
    /// A string containing the root namespace.
    /// </returns>
    public string? GetRootNamespace()
    {
        string? rootNamespace = null;

        if (Properties is not null)
        {
            bool hasValue = Properties.TryGetValue("RootNamespace", out string? root);
            if (hasValue)
            {
                rootNamespace = root;
            }
        }
        return rootNamespace;
    }

    public List<string>? GetTargetFrameworkList()
    {
        List<string>? frameworkList = null;

        if (Properties is null)
        {
            return null;
        }
        else
        {
            bool hasValue = Properties.TryGetValue("TargetFramework", out string? framework);
            if (hasValue && framework is not null)
            {
                frameworkList = new List<string> { framework };
            }
        }
        return frameworkList;
    }
}
