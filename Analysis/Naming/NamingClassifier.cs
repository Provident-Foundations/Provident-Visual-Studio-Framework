using System.Text;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Tokenizes identifiers and classifies their naming styles.</summary>
public interface INamingClassifier
{
    /// <summary>Splits an identifier into semantic tokens.</summary>
    NamingTokenization Tokenize(string name);
    /// <summary>Classifies an identifier's naming characteristics.</summary>
    NamingClassification Classify(string name);
}

/// <summary>Provides naming tokenization and style classification.</summary>
public sealed class NamingClassifier : INamingClassifier
{
    /// <inheritdoc />
    public NamingTokenization Tokenize(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (name.Length == 0)
            return new NamingTokenization();

        List<string> tokens = new();
        List<char> separators = new();
        StringBuilder current = new();
        for (int index = 0; index < name.Length; index++)
        {
            char character = name[index];
            if (!char.IsLetterOrDigit(character))
            {
                AddToken(tokens, current);
                if (!separators.Contains(character))
                    separators.Add(character);
                continue;
            }

            bool startsWord = current.Length > 0 && char.IsUpper(character) &&
                (char.IsLower(current[^1]) || (index + 1 < name.Length && char.IsLower(name[index + 1]) && char.IsUpper(current[^1])));
            if (startsWord)
                AddToken(tokens, current);
            current.Append(character);
        }
        AddToken(tokens, current);

        string? prefix = tokens.Count > 1 ? tokens[0] : null;
        string? suffix = tokens.Count > 1 ? tokens[^1] : null;
        bool acronym = tokens.Any(token => token.Length > 1 && token.All(char.IsUpper));
        bool numeric = tokens.Any(token => token.Any(char.IsDigit));
        return new NamingTokenization
        {
            Original = name,
            Tokens = tokens,
            Prefix = prefix,
            Suffix = suffix,
            ContainsAcronym = acronym,
            ContainsNumericToken = numeric,
            Separators = separators
        };
    }

    /// <inheritdoc />
    public NamingClassification Classify(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        NamingTokenization tokenization = Tokenize(name);
        string value = name.TrimStart('_');
        NamingStyle style = DetermineStyle(name, value, tokenization);
        bool leadingUnderscore = name.StartsWith('_');
        return new NamingClassification
        {
            Style = style,
            Confidence = style == NamingStyle.Unknown || style == NamingStyle.Mixed ? 0.35 : 0.95,
            Tokenization = tokenization,
            HasLeadingUnderscore = leadingUnderscore,
            HasAsyncSuffix = name.EndsWith("Async", StringComparison.Ordinal),
            AppearsAbbreviated = tokenization.ContainsAcronym,
            Length = name.Length
        };
    }

    private static void AddToken(List<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
            return;
        tokens.Add(current.ToString());
        current.Clear();
    }

    private static NamingStyle DetermineStyle(string original, string value, NamingTokenization tokenization)
    {
        if (original.Length == 0)
            return NamingStyle.Unknown;
        if (original.Contains(' '))
            return NamingStyle.SpaceSeparated;
        if (original.Contains('-'))
            return NamingStyle.KebabCase;
        if (original.Contains('.'))
            return NamingStyle.DotSeparated;
        if (original.Contains('_'))
        {
            if (original.StartsWith('_'))
                return NamingStyle.UnderscorePrefixed;
            return value.Length > 0 && value.All(character => char.IsUpper(character) || character == '_')
                ? NamingStyle.ScreamingSnakeCase
                : NamingStyle.SnakeCase;
        }
        if (value.Length == 0 || !value.Any(char.IsLetter))
            return NamingStyle.Unknown;
        if (char.IsUpper(value[0]))
            return NamingStyle.PascalCase;
        if (char.IsLower(value[0]))
            return NamingStyle.CamelCase;
        return tokenization.Tokens.Count > 1 ? NamingStyle.Mixed : NamingStyle.Unknown;
    }
}
