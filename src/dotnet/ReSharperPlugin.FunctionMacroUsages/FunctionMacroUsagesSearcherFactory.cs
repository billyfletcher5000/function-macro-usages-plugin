using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Feature.Services.Cpp.DeclaredElements;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.Resolve;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using JetBrains.ReSharper.Psi.ExtensionsAPI;

namespace ReSharperPlugin.ContextActions;


[PsiComponent(Instantiation.DemandAnyThreadSafe)]
public class FunctionMacroUsagesSearcherFactory : DomainSpecificSearcherFactoryBase
{
    public override IEnumerable<RelatedDeclaredElement> GetRelatedDeclaredElements(IDeclaredElement element)
    {
        List<ICppResolveEntity> additionalFunctions = new List<ICppResolveEntity>();
        ICppResolveEntity fromDeclaredElement = element.GetResolveEntityFromDeclaredElement();
        if (fromDeclaredElement != null)
        {
            string baseElementName = fromDeclaredElement.Name.ToString();
        
            HashSet<string> macroFunctionNames = new HashSet<string>();
            macroFunctionNames.Add($"Get{baseElementName}");
            macroFunctionNames.Add($"GetRef{baseElementName}");
            macroFunctionNames.Add($"Set{baseElementName}");
            macroFunctionNames.Add($"Update{baseElementName}");
        
            ICppClassResolveEntity classResolveEntity = fromDeclaredElement.GetEnclosingClass(true);
            if (classResolveEntity != null)
            {
                CppList<ICppResolveEntity> children = classResolveEntity.GetChildren();
                foreach (ICppResolveEntity child in children)
                {
                    CppDeclaratorResolveEntityPack pack = child as CppDeclaratorResolveEntityPack;
                    if (pack != null)
                    {
                        string childName = child.Name.ToString();
                        
                        if (macroFunctionNames.Contains(childName))
                        {
                            CppSmallEnumerable<CppGroupedFunctionDeclaratorResolveEntity> groupedFunctions = pack.GetGroupedFunctions();

                            foreach (CppGroupedFunctionDeclaratorResolveEntity groupedFunction in pack.GetGroupedFunctions())
                                additionalFunctions.Add(groupedFunction);
                            
                            foreach (ICppMutableFunctionTemplateDeclaratorResolveEntity funcTemplate in pack.GetFunctionTemplates())
                                additionalFunctions.Add(funcTemplate);
                        }
                    }
                }
            }
        }

        var declaredElements = CppResolveEntityDeclaredElement.CreateWrappers(element.GetPsiServices(), additionalFunctions);
        List<RelatedDeclaredElement> relatedDeclaredElements = new List<RelatedDeclaredElement>();
        foreach (IDeclaredElement declaredElement in declaredElements)
            relatedDeclaredElements.Add(new RelatedDeclaredElement(declaredElement));
        
        return relatedDeclaredElements;
    }

    public override bool IsCompatibleWithLanguage(PsiLanguageType languageType)
    {
        return languageType.Is<CppLanguage>();
    }
}