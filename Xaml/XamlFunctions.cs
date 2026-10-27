using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Provident.Foundations.VisualStudio.Xaml;

/// <summary>
/// Provides shared methods and functions for operating on XAML code/text.
/// </summary>
public static class XamlFunctions
{
    /// <summary>
    /// Attempts to read the specified attribute value from the element.
    /// </summary>
    /// <param name="element">
    /// The <see cref="XElement"/> instance whose attributes are being examined.
    /// </param>
    /// <returns>
    /// A string containing the value of the attribute if present; otherwise, returns <b>null</b>.
    /// </returns>
    public static string? ReadAttributeValueFromElement(XElement element, string attributeName)
    {
        // Look for the attribute with the specified name in the original XAML code.
        XAttribute? attribute = element.Attributes()
            .FirstOrDefault(
                attribute => string.Compare(attribute.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase) == 0);

        if (attribute is not null)
        {
            // If the value is present, read the value and return it.
            return attribute.Value.Trim();
        }

        return null;
    }

    /// <summary>
    /// Reads the attribute object from the specified element.
    /// </summary>
    /// <param name="element">
    /// The <see cref="XElement"/> instance to read the attribute from.
    /// </param>
    /// <param name="attributeName">
    /// A string containing the name of the attribute to be read.
    /// </param>
    /// <returns>
    /// The matching <see cref="XAttribute"/> instance, if found; otherwise, returns
    /// <b>null</b>.
    /// </returns>
    public static XAttribute? ReadAttributeFromElement(XElement element, string attributeName)
    {
        // Look for the attribute with the specified name in the original XAML code.
        return element.Attributes()
            .FirstOrDefault(
                attribute => string.Compare(attribute.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase) == 0);
    }


    public static void UpdateStyleDependencies(XamlDefinitionsContainer definitions, string attributeValue)
    {
        // Find the matches for the "BasedOn" attribute value using regular expressions.
        MatchCollection matchList = XamlRegEx.ResourceReferenceRegex.Matches(attributeValue);

        foreach (Match match in matchList)
        {
            // Extract the parent key value.
            string parentKey = match.Groups[1].Value.Trim();
            definitions.UpdateStyleDependencies(parentKey);
        }

    }
}
