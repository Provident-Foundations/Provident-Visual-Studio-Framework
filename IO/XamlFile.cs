using Provident.Foundations.Abstractions;
using Provident.Foundations.Common;
using System.Xml.Linq;

namespace Provident.Foundations.VisualStudio.IO;

/// <summary>
/// Represents and contains the content of a XAML file.
/// </summary>
public sealed class XamlFile : DisposableObjectBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlFile"/> class.
    /// </summary>
    public XamlFile()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlFile"/> class.
    /// </summary>
    /// <param name="pathAndFileName">The fully qualified file path.</param>
    public XamlFile(string pathAndFileName)
    {
        FileName = pathAndFileName;
    }

    /// <summary>
    /// Gets or sets the fully qualified file path.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets the parsed XML document.
    /// </summary>
    public XDocument? Document { get; private set; }

    /// <summary>
    /// Gets the source text split into lines.
    /// </summary>
    public List<string>? Lines { get; private set; }

    /// <summary>
    /// Gets the source text.
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// Loads and parses the XAML file.
    /// </summary>
    /// <returns>The result of the load operation.</returns>
    public OperationResult Load()
    {
        OperationResult result = new();
        Document = null;
        Lines = null;
        Text = null;

        if (string.IsNullOrWhiteSpace(FileName) || !File.Exists(FileName))
        {
            result.SetFailureMessage($"Could not find file: '{FileName}'.");
            return result;
        }

        try
        {
            Text = File.ReadAllText(FileName);
            Lines = Text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None).ToList();
        }
        catch (Exception exception)
        {
            result.SetFailureMessage($"Failed to read the file: '{FileName}'.");
            result.AddException(exception);
            return result;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            result.SetFailureMessage($"File is empty: '{FileName}'.");
            return result;
        }

        try
        {
            Document = XDocument.Parse(Text, LoadOptions.PreserveWhitespace);
            result.Success = true;
        }
        catch (Exception exception)
        {
            result.SetFailureMessage($"File is not an XML file. '{FileName}'.");
            result.AddException(exception);
        }

        return result;
    }

    /// <summary>
    /// Loads and parses the XAML file.
    /// </summary>
    /// <returns>The result of the load operation.</returns>
    public async Task<OperationResult> LoadAsync()
    {
        OperationResult result = new();
        Document = null;
        Lines = null;
        Text = null;

        if (string.IsNullOrWhiteSpace(FileName) || !File.Exists(FileName))
        {
            result.SetFailureMessage($"Could not find file: '{FileName}'.");
            return result;
        }

        try
        {
            Text = await File.ReadAllTextAsync(FileName).ConfigureAwait(false);
            Lines = Text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None).ToList();
        }
        catch (Exception exception)
        {
            result.SetFailureMessage($"Failed to read the file: '{FileName}'.");
            result.AddException(exception);
            return result;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            result.SetFailureMessage($"File is empty: '{FileName}'.");
            return result;
        }

        try
        {
            Document = XDocument.Parse(Text, LoadOptions.PreserveWhitespace);
            result.Success = true;
        }
        catch (Exception exception)
        {
            result.SetFailureMessage($"File is not an XML file. '{FileName}'.");
            result.AddException(exception);
        }

        return result;
    }
}
