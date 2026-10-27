using Provident.Foundations.Abstractions;
using Provident.Foundations.Common;
using Provident.Foundations.VisualStudio.IO;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Contains a result of the validation process for a XAML project.
/// </summary>
public sealed class XamlProjectValidationResult : DisposableObjectBase
{
    #region Private Member Declarations
    /// <summary>
    /// The list of XAML files.
    /// </summary>
    private Dictionary<string, XamlFile>? _xamlFiles;

    /// <summary>
    /// The log containing the validation results.
    /// </summary>
    private OperationLog? _log;
    #endregion

    #region Constructor / Dipose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlProjectValidationResult"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    public XamlProjectValidationResult()
    {
        _xamlFiles = new Dictionary<string, XamlFile>(StringComparer.OrdinalIgnoreCase);
        _log = new OperationLog();
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="XamlProjectValidationResult"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            _log?.Dispose();
            _xamlFiles?.Clear();
        }

        _xamlFiles = null;
        _log = null;

        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the reference to the list of XAML file definitions used in the validation.
    /// </summary>
    /// <value>
    /// A <see cref="Dictionary{TKey, TValue}"/>.
    /// </value>
    public Dictionary<string, XamlFile> XamlFiles
    {
        get
        {
            ObjectDisposedException.ThrowIf(_xamlFiles is null, this);
            return _xamlFiles!;
        }
    }

    /// <summary>
    /// Gets the reference to the operation log in use.
    /// </summary>
    public OperationLog Log
    {
        get
        {
            ObjectDisposedException.ThrowIf(_log is null, this);
            return _log!;
        }
    }
    #endregion

    #region Public Methods / Functions
    /// <summary>
    /// Sets the content of the XAML file list to the provided content.
    /// </summary>
    /// <param name="xamlFiles">
    /// A <see cref="Dictionary{TKey, TValue}"/> where the string key is the file name
    /// and the value is the <see cref="XamlFile"/> instance.
    /// </param>
    public void SetXamlFileList(Dictionary<string, XamlFile>? xamlFiles)
    {
        _xamlFiles = new Dictionary<string, XamlFile>();
        if (xamlFiles is not null)
        {
            foreach(string key in xamlFiles.Keys)
            {
                _xamlFiles.Add(key, xamlFiles[key]);
            }
        }
    }
    #endregion
}
