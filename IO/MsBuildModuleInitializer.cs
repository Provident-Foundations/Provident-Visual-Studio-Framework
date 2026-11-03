using Microsoft.Build.Locator;
using System.Runtime.CompilerServices;

namespace Provident.Foundations.VisualStudio.IO;

internal static class MsBuildModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
}
