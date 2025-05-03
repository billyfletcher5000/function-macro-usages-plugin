using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace ReSharperPlugin.SearchPatternUsages.Tests;

[ZoneDefinition]
public class SearchPatternUsagesTestEnvironmentZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>, IRequire<ISearchPatternUsagesZone> { }

[ZoneMarker]
public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>, IRequire<SearchPatternUsagesTestEnvironmentZone> { }

[SetUpFixture]
public class SearchPatternUsagesTestsAssembly : ExtensionTestEnvironmentAssembly<SearchPatternUsagesTestEnvironmentZone> { }
