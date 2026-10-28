using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Xaml;

/// <summary>
/// Provides the base definition for defining and representing XAML resources.
/// </summary>
public abstract class XamlResourceDescriptorBase : DisposableObjectBase
{
    #region Private Member Declarations
    /// <summary>
    /// The key / ID value for the resource.
    /// </summary>
    private string? _key;

    /// <summary>
    /// The element type.
    /// </summary>
    private string? _elementType;

    /// <summary>
    /// The file in which the item is defined.
    /// </summary>
    private string? _file;
    #endregion

    #region Constructor / Dispose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlResourceDescriptorBase"/> class. 
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    protected XamlResourceDescriptorBase()
    {
        _key = string.Empty;
        _elementType = string.Empty;
        _file = string.Empty;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="XamlResourceDescriptorBase"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        _key = null;
        _elementType = null;
        _file = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets or sets the resource key.
    /// </summary>
    /// <value>
    /// A string containing the unqiue key value used to identify the resource,
    /// </value>
    public string Key
    {
        get
        {
            _key ??= string.Empty;
            return _key;
        }
        set
        {
            _key = value;
        }
    }

    /// <summary>
    /// Gets or sets the source file.
    /// </summary>
    /// <value>
    /// A string containing the path and name of the file.
    /// </value>
    public virtual string File
    {
        get
        {
            _file ??= string.Empty;
            return _file;
        }
        set
        {
            _file = value;
        }
    }

    /// <summary>
    /// Gets or sets the XML element type.
    /// </summary>
    /// <value>
    /// A string specifying the element type for the resource.
    /// </value>
    public string ElementType
    {
        get
        {
            _elementType ??= string.Empty;
            return _elementType;
        }
        set
        {
            _elementType = value;
        }
    }
    #endregion
}
