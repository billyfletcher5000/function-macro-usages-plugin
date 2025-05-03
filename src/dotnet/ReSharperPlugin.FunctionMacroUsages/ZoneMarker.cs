using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.FunctionMacroUsages;

[ZoneMarker]
public class ZoneMarker : IRequire<ILanguageCppZone>
{
}