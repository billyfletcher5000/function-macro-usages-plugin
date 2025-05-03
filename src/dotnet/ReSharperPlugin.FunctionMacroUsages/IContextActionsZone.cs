using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.ContextActions;

[ZoneDefinition]
// [ZoneDefinitionConfigurableFeature("Title", "Description", IsInProductSection: false)]
public interface IContextActionsZone : IZone,
    IRequire<ILanguageCppZone>
{
}