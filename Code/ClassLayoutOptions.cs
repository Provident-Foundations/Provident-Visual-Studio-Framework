using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Code;

/// <summary>
/// Contains and specifies the options for the layout of a class definition.
/// </summary>
public sealed class ClassLayoutOptions : DisposableObjectBase
{
    private List<RegionDefinition>? _regions;

    public ClassLayoutOptions()
    {
        _regions = [];
    }
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            _regions?.Clear();
        }

        _regions = null;
        base.Dispose(disposing);
    }

    public List<RegionDefinition> Regions
    {
        get => _regions ?? [];
        set
        {
            _regions = value;
        }
    }
}
