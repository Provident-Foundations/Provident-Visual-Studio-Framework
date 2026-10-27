using System.Text.RegularExpressions;

namespace Provident.Foundations.VisualStudio.Xaml;

/// <summary>
/// Contains the constant definitions for RegEx operations used with XAML.
/// </summary>
public static  class XamlRegEx
{
    /// <summary>
    /// Resource Reference regular expression.
    /// </summary>
    public static readonly Regex ResourceReferenceRegex = new(
            @"{(?:StaticResource|ThemeResource)\s+([^}]+)}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

}
