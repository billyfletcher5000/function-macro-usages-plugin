using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Application.Parts;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Cpp.DeclaredElements;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using ReSharperPlugin.FunctionMacroUsages;

namespace ReSharperPlugin.ContextActions;

[PsiComponent(Instantiation.DemandAnyThreadSafe)]
public class FunctionMacroUsagesSearcherFactory : DomainSpecificSearcherFactoryBase
{
    private readonly ISettingsStore mySettingsStore;

    public FunctionMacroUsagesSearcherFactory(ISettingsStore settingsStore)
    {
        mySettingsStore = settingsStore;
    }

    public override IEnumerable<RelatedDeclaredElement> GetRelatedDeclaredElements(IDeclaredElement element)
    {
        var additionalEntities = new List<ICppResolveEntity>();
        var fromDeclaredElement = element.GetResolveEntityFromDeclaredElement();
        if (fromDeclaredElement != null)
        {
            var baseElementName = fromDeclaredElement.Name.ToString();

            var boundSettingsStore = mySettingsStore.BindToContextTransient(ContextRange.ApplicationWide);
            var searchEntrySettings = FunctionMacroUsagesSearchEntrySettingKeyAccessor.GetSearchEntrySettings(boundSettingsStore);
            Regex regex = new Regex("(?i){Foo}");

            var classResolveEntity = fromDeclaredElement.GetEnclosingClass(true);
            if (classResolveEntity != null)
            {
                var children = classResolveEntity.GetChildren();
                foreach (var child in children)
                {
                    var pack = child as CppDeclaratorResolveEntityPack;
                    if (pack != null)
                    {
                        var childName = child.Name.ToString();

                        foreach (FunctionMacroUsagesSearchEntrySetting searchEntrySetting in searchEntrySettings)
                        {
                            string replaced = regex.Replace(searchEntrySetting.SearchPattern, baseElementName);
                            if (replaced != searchEntrySetting.SearchPattern && String.Equals(replaced, childName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (searchEntrySetting.SearchFunctions)
                                {
                                    foreach (var groupedFunction in pack.GetGroupedFunctions())
                                        additionalEntities.Add(groupedFunction);
                                }
                                if (searchEntrySetting.SearchFunctionTemplates)
                                {
                                    foreach (var funcTemplate in pack.GetFunctionTemplates())
                                        additionalEntities.Add(funcTemplate);
                                }
                                
                                if (searchEntrySetting.SearchVariables)
                                {
                                    foreach (var variable in pack.GetGroupedVariables())
                                        additionalEntities.Add(variable);
                                }
                                if (searchEntrySetting.SearchVariableTemplates)
                                {
                                    foreach (var varTemplate in pack.GetVariableTemplates())
                                        additionalEntities.Add(varTemplate);
                                }
                                
                                if (searchEntrySetting.SearchTypeAliases)
                                {
                                    foreach (var alias in pack.GetGroupedTypeAliases())
                                        additionalEntities.Add(alias);
                                }
                                if (searchEntrySetting.SearchTypeAliasTemplates)
                                {
                                    foreach (var aliasTemplate in pack.GetTypeAliasTemplates())
                                        additionalEntities.Add(aliasTemplate);
                                }
                            }
                        }
                    }
                }
            }
        }

        var declaredElements =
            CppResolveEntityDeclaredElement.CreateWrappers(element.GetPsiServices(), additionalEntities);
        var relatedDeclaredElements = new List<RelatedDeclaredElement>();
        foreach (var declaredElement in declaredElements)
            relatedDeclaredElements.Add(new RelatedDeclaredElement(declaredElement));

        return relatedDeclaredElements;
    }

    public override bool IsCompatibleWithLanguage(PsiLanguageType languageType)
    {
        return languageType.Is<CppLanguage>();
    }
}