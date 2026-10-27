using Provident.Foundations.VisualStudio.Model;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using System.Xml.Linq;

namespace Provident.Foundations.VisualStudio.IO;

/// <summary>
/// Loads MSBuild project files into <see cref="ProjectModel"/> instances.
/// </summary>
public sealed class VisualStudioProjectLoader
{
    private static readonly HashSet<string> SourceItemTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Compile",
        "None",
        "Content",
        "EmbeddedResource",
    };

    /// <summary>
    /// Loads a project file and its declared MSBuild metadata.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <returns>The populated project model.</returns>
    public ProjectModel Load(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
        string fullPath = Path.GetFullPath(projectPath);
        XDocument document = XDocument.Load(fullPath, LoadOptions.PreserveWhitespace);
        XElement projectElement =
            document.Root
            ?? throw new InvalidDataException("The project file has no root element.");
        ProjectModel project = new()
        {
            FilePath = fullPath,
            Name = Path.GetFileNameWithoutExtension(fullPath),
            Sdk = ParseSdk(projectElement.Attribute("Sdk")?.Value),
        };

        foreach (XElement propertyGroup in Elements(projectElement, "PropertyGroup"))
        {
            PropertyGroupModel group = new()
            {
                Condition = AttributeValue(propertyGroup, "Condition"),
            };
            foreach (XElement property in propertyGroup.Elements())
            {
                string value = property.Value.Trim();
                group.Properties[property.Name.LocalName] = value;
                project.Properties[property.Name.LocalName] = value;
            }

            project.PropertyGroups.Add(group);
            SetProjectProperties(project, group.Properties);
        }

        foreach (XElement itemGroupElement in Elements(projectElement, "ItemGroup"))
        {
            ItemGroupModel itemGroup = new()
            {
                Condition = AttributeValue(itemGroupElement, "Condition"),
            };
            foreach (XElement itemElement in itemGroupElement.Elements())
            {
                ProjectItemModel item = ParseItem(itemElement);
                itemGroup.Items.Add(item);
                AddSpecializedItem(project, itemElement.Name.LocalName, item);
            }

            project.ItemGroups.Add(itemGroup);
        }

        LoadEvaluatedSourceItems(project, fullPath);

        return project;
    }

    private static void LoadEvaluatedSourceItems(ProjectModel project, string projectPath)
    {
        project.CodeFiles.Clear();
        project.XamlFiles.Clear();

        // Microsoft.Build's SDK resolvers are not initialized when the API is
        // hosted by an application instead of MSBuild.exe or Visual Studio.
        // Register the installed MSBuild instance before creating a project
        // collection so SDK-style projects can resolve Microsoft.NET.Sdk.
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        using ProjectCollection projectCollection = new();
        Project evaluatedProject = projectCollection.LoadProject(projectPath);

        foreach (string itemType in SourceItemTypes
            .Concat(["Page", "ApplicationDefinition", "XamlResource"]))
        {
            foreach (ProjectItem evaluatedItem in evaluatedProject.GetItems(itemType))
            {
                ProjectItemModel item = new()
                {
                    ItemType = evaluatedItem.ItemType,
                    Include = evaluatedItem.EvaluatedInclude,
                };

                foreach (ProjectMetadata metadata in evaluatedItem.Metadata)
                {
                    item.Metadata[metadata.Name] = metadata.EvaluatedValue;
                }

                AddSpecializedItem(project, evaluatedItem.ItemType, item);
            }
        }
    }

    private static ProjectSdkModel ParseSdk(string? sdk)
    {
        if (string.IsNullOrWhiteSpace(sdk))
        {
            return new ProjectSdkModel();
        }

        string[] parts = sdk.Split('/', 2, StringSplitOptions.TrimEntries);
        return new ProjectSdkModel
        {
            Name = parts[0],
            Version = parts.Length == 2 ? parts[1] : null,
        };
    }

    private static ProjectItemModel ParseItem(XElement element)
    {
        ProjectItemModel item = new()
        {
            ItemType = element.Name.LocalName,
            Include = AttributeValue(element, "Include"),
            Update = AttributeValue(element, "Update"),
            Remove = AttributeValue(element, "Remove"),
        };

        foreach (XElement metadata in element.Elements())
        {
            item.Metadata[metadata.Name.LocalName] = metadata.Value.Trim();
        }

        return item;
    }

    private static void AddSpecializedItem(
        ProjectModel project,
        string itemType,
        ProjectItemModel item
    )
    {
        string? include = item.Include;
        if (string.IsNullOrWhiteSpace(include))
        {
            return;
        }

        switch (itemType)
        {
            case "PackageReference":
                project.PackageReferences.Add(
                    new PackageReferenceModel
                    {
                        Include = include,
                        Version = item.Metadata.GetValueOrDefault("Version"),
                        PrivateAssets = string.Equals(
                            item.Metadata.GetValueOrDefault("PrivateAssets"),
                            "all",
                            StringComparison.OrdinalIgnoreCase
                        ),
                    }
                );
                break;
            case "ProjectReference":
                project.ProjectReferences.Add(
                    new ProjectReferenceModel
                    {
                        Include = include,
                        ProjectGuid = item.Metadata.GetValueOrDefault("Project")?.Trim('{', '}'),
                        Name = item.Metadata.GetValueOrDefault("Name"),
                    }
                );
                break;
            case "Page" when include.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase):
            case "ApplicationDefinition":
            case "XamlResource":
                project.XamlFiles.Add(new XamlFileModel { Include = include, ItemType = itemType });
                break;
            default:
                if (
                    SourceItemTypes.Contains(itemType)
                    && !include.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)
                )
                {
                    project.CodeFiles.Add(
                        new CodeFileModel
                        {
                            Include = include,
                            ItemType = itemType,
                            Language = Path.GetExtension(include).TrimStart('.'),
                        }
                    );
                }
                break;
        }
    }

    private static void SetProjectProperties(
        ProjectModel project,
        Dictionary<string, string> properties
    )
    {
        if (properties.TryGetValue("ProjectGuid", out string? projectGuid))
        {
            project.ProjectGuid = projectGuid;
        }

        if (properties.TryGetValue("TargetFramework", out string? targetFramework))
        {
            project.Configurations.Add(
                new ProjectConfigurationModel { TargetFramework = targetFramework }
            );
        }

        if (properties.TryGetValue("TargetFrameworks", out string? targetFrameworks))
        {
            foreach (
                string framework in targetFrameworks.Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
            )
            {
                project.Configurations.Add(
                    new ProjectConfigurationModel { TargetFramework = framework }
                );
            }
        }

        if (properties.TryGetValue("ApplicationManifest", out string? manifest))
        {
            project.Packaging.ManifestPath = manifest;
        }

        if (properties.TryGetValue("WindowsAppSDKVersion", out string? appSdkVersion))
        {
            project.Packaging.WindowsAppSdkVersion = appSdkVersion;
        }
    }

    private static IEnumerable<XElement> Elements(XElement parent, string localName) =>
        parent
            .Elements()
            .Where(element =>
                element.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase)
            );

    private static string? AttributeValue(XElement element, string name) =>
        element
            .Attributes()
            .FirstOrDefault(attribute =>
                attribute.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)
            )
            ?.Value;
}
