using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.SearchPatternUsages
{
    [ZoneDefinition]
    public interface ISearchPatternUsagesZone : IZone, IRequire<ILanguageCppZone>
    {
    }
}