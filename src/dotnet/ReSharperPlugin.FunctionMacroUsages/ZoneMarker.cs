using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.ContextActions;

[ZoneMarker]
public class ZoneMarker : IRequire<ILanguageCppZone> { }
