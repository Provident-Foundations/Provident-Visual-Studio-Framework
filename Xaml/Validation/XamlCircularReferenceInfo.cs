using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Contains the stack of cycles detected in the style dependency graph.
/// </summary>
public sealed class XamlCircularReferenceInfo : DisposableObjectBase
{
    #region Private Member Declarations
    private List<List<string>>? _cycles;
    private HashSet<string>? _visited;
    private HashSet<string>? _stack;
    private Stack<string>? _path;
    #endregion

    #region Constructor / Dispose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlCircularReferenceInfo"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    public XamlCircularReferenceInfo()
    {
        _cycles = [];
        _visited = [];
        _stack = [];
        _path = [];
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="CircularReferenceInfo"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cycles?.Clear();
            _visited?.Clear();
            _stack?.Clear();
            _path?.Clear();
        }

        _cycles = null;
        _visited = null;
        _stack = null;
        _path = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the reference to the list of detected cycles.
    /// </summary>
    public List<List<string>>? Cycles => _cycles;

    /// <summary>
    /// Gets the reference to the list of nodes visited during traversal.
    /// </summary>
    public HashSet<string>? Visited => _visited;

    /// <summary>
    /// Gets the reference to the list of nodes on the active traversal stack.
    /// </summary>
    public HashSet<string>? Stack => _stack;

    /// <summary>
    /// Gets the reference to the current traversal path stack.
    /// </summary>
    public Stack<string>? Path => _path;

    /// <summary>
    /// Gets the number of cycles.
    /// </summary>
    public int CycleCount
    {
        get
        {
            return _cycles?.Count ?? 0;
        }
    }
    #endregion
}
