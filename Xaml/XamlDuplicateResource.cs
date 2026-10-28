namespace Provident.Foundations.VisualStudio.Xaml;

/// <summary>
/// Represents a duplicated XAML resource entry.
/// </summary>
public sealed class XamlDuplicateResource : XamlResourceDescriptorBase
{
    #region Private Member Declarations
    /// <summary>
    /// The file where the first version of the item is defined.
    /// </summary>
    private string? _existingFile;

    /// <summary>
    /// The name of the file where the item is duplicated.
    /// </summary>
    private string? _duplicateFile;
    #endregion

    #region Constructor / Dispose Methods
    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="XamlDuplicateResource"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        _existingFile = null;
        _duplicateFile = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    public override string File
    {
        get => _duplicateFile;
        set
        {
            base.File = value;
            _duplicateFile = value;
        }

    }
    /// <summary>
    /// Gets or sets the name of the file where the item is duplicated.
    /// </summary>
    /// <value>
    /// A string containing the XAML file name, or <see cref="string.Empty"/>.
    /// </value>
    public string DuplicateFile
    {
        get
        {
            return _duplicateFile ?? string.Empty;
        }
        set
        {
            _duplicateFile = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the file where the original version is defined.
    /// </summary>
    /// <value>
    /// A string containing the XAML file name, or <see cref="string.Empty"/>.
    /// </value>
    public string ExistingFile
    {
        get
        {
            return _existingFile ?? string.Empty;
        }
        set
        {
            _existingFile = value;
        }
    }
    #endregion
}
