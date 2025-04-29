using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp;

namespace ReSharperPlugin.ContextActions;

[ZoneDefinition]
// [ZoneDefinitionConfigurableFeature("Title", "Description", IsInProductSection: false)]
public interface IContextActionsZone : IZone,
    IRequire<ILanguageCppZone>
{
}
