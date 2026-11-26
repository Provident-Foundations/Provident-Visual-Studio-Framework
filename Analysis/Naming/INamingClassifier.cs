namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Tokenizes identifiers and classifies their naming styles.
/// </summary>
public interface INamingClassifier
{
    /// <summary>
    /// Splits an identifier into semantic tokens.
    /// </summary>
    NamingTokenization Tokenize(string name);

    /// <summary>
    /// Classifies an identifier's naming characteristics.
    /// </summary>
    NamingClassification Classify(string name);
}
