namespace Provident.Foundations.VisualStudio.Xaml;

/// <summary>
/// Represents a reference to a keyed XAML resource.
/// </summary>
public class XamlResourceReference : XamlResourceDescriptorBase
{
    /// <summary>
    /// Gets or sets the one-based source line number of the XAML code in which the reference occurs.
    /// </summary>
    /// <value>
    /// An integer indicating the line number.
    /// </value>
    public int Line { get; set; }

}
