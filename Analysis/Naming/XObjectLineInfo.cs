using System.Xml;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

internal sealed class XObjectLineInfo : Provident.Foundations.Abstractions.DisposableObjectBase, IXmlLineInfo
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public bool HasLineInfo() => false;

    public int LineNumber => 1;

    public int LinePosition => 1;
}
