using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Contains the summary data for the validation.
/// </summary>
public sealed class XamlValidationSummary : DisposableObjectBase
{
    #region Private Member Declarations
    private int _definitionsCount;
    private int _referencesCount;
    private int _duplicatesCount;
    private int _missingKeysCount;
    private int _circularStyleCount;
    #endregion

    #region Constructor / Dispose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlValidationSummary"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    public XamlValidationSummary()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlValidationSummary"/> class.
    /// </summary>
    /// <param name="definitions">
    /// The reference to the <see cref="XamlDefinitionsContainer"/> containing all the definitions for the related XAML files.
    /// </param>
    /// <param name="circularInfo">
    /// The <see cref="XamlCircularReferenceInfo"/> instance containing the circular reference information.
    /// </param>
    public XamlValidationSummary(XamlDefinitionsContainer? definitions, XamlCircularReferenceInfo? circularInfo)
    {
        if (definitions is not null)
        {
            _definitionsCount = definitions.ResourceDefinitions?.Count ?? 0;
            _referencesCount = definitions.References?.Count ?? 0;
            _duplicatesCount = definitions.DuplicateDefinitions?.Count ?? 0;
            _missingKeysCount = definitions.MissingResources?.Count ?? 0;
        }
        else
        {
            _definitionsCount = 0;
            _referencesCount = 0;
            _duplicatesCount = 0;
            _missingKeysCount = 0;
        }
        if (circularInfo is not null)
        {
            _circularStyleCount = circularInfo.CycleCount;
        }
        else
        {
            _circularStyleCount = 0;
        }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the number of XAML style circular references that were found.
    /// </summary>
    /// <value>
    /// An integer value specifying the number of circular references.
    /// </value>
    public int CircularStyleCount => _circularStyleCount;

    /// <summary>
    /// Gets the number of XAML style definitions that were found.
    /// </summary>
    /// <value>
    /// An integer value specifying the number of definitions.
    /// </value>
    public int DefinitionsCount => _definitionsCount;

    /// <summary>
    /// Gets the number of duplicate XAML style definitions that were found.
    /// </summary>
    /// <value>
    /// An integer value specifying the number of duplicates.
    /// </value>
    public int DuplicatesCount => _duplicatesCount;

    /// <summary>
    /// Gets the number of missing XAML style definitions that were found.
    /// </summary>
    /// <value>
    /// An integer value specifying the number of missing styles.
    /// </value>
    public int MissingKeysCount => _missingKeysCount;

    /// <summary>
    /// Gets the number of XAML style references that were found.
    /// </summary>
    /// <value>
    /// An integer value specifying the number of references.
    /// </value>
    public int ReferencesCount => _referencesCount;
    #endregion

    #region
    #endregion

}



