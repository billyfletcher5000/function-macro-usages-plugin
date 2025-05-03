using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.SearchPatternUsages
{
    [ZoneMarker]
    public class ZoneMarker : IRequire<ILanguageCppZone>
    {
    }
}