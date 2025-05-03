using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.FunctionMacroUsages;

[ZoneDefinition]
public interface IContextActionsZone : IZone, IRequire<ILanguageCppZone>
{
}