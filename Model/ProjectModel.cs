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
}
