using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Feature.Services.Cpp.DeclaredElements;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Features.Navigation.Features.FindUsages;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Cpp;
using JetBrains.ReSharper.Psi.Cpp;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.Resolve;
using JetBrains.ReSharper.Psi.Cpp.Symbols;

[ShellFeaturePart(Instantiation.DemandAnyThreadSafe)]
class FunctionMacroFindUsagesContextSearch : FindUsagesContextSearch
{
  protected override IEnumerable<DeclaredElementInstance> GetElementCandidates(
    IDataContext context,
    ReferencePreferenceKind kind,
    bool updateOnly)
  {
    IEnumerable<IDeclaredElement> elements = CppContextSearchUtil.GetDeclaredElements(context, kind);
    List<IDeclaredElement> elementsList = elements.ToList();
    List<ICppResolveEntity> additionalFunctions = new List<ICppResolveEntity>();
    IPsiServices services = null;
    
    foreach(IDeclaredElement element in elementsList)
    {
      ICppResolveEntity fromDeclaredElement = element.GetResolveEntityFromDeclaredElement();
      if (fromDeclaredElement != null)
      {
        services = element.GetPsiServices();
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
            string childName = child.Name.ToString();
            bool isFunction = child.IsFunctionDeclaratorOrTemplateFunctionPack();
            TriBool isGeneratedOrBuiltinResolveEntity = pack.IsGeneratedOrBuiltinResolveEntity();
            var groupedEntities = pack.GetGroupedEntities();
            CppSmallEnumerable<CppGroupedFunctionDeclaratorResolveEntity> groupedFunctions = pack.GetGroupedFunctions();
            
            var groupedVariables = pack.GetGroupedVariables();
            if (macroFunctionNames.Contains(childName))
            {
              foreach (CppGroupedFunctionDeclaratorResolveEntity groupedFunction in groupedFunctions)
              {
                additionalFunctions.Add(groupedFunction);
              }
            }
          }
        }
      }
    }

    if (services != null)
    {
      IList<IDeclaredElement> additionalElements = CppResolveEntityDeclaredElement.CreateWrappers(services, additionalFunctions);
      return additionalElements.Select<IDeclaredElement, DeclaredElementInstance>((Func<IDeclaredElement, DeclaredElementInstance>)(e => new DeclaredElementInstance(e)));
    }

    return new List<DeclaredElementInstance>();
  }

  public override bool IsContextApplicable(IDataContext dataContext)
  {
    return CppContextSearchUtil.IsContextSearchApplicable(dataContext, this.ReferencePreferenceKind);
  }

  protected override ICollection<DeclaredElementInstance> Promote(
    ICollection<DeclaredElementInstance> elements)
  {
    return CppDeclaredElementUtil.FindBaseAndOverridingDeclaredElements(elements);
  }

  protected override IOccurrence Present(DeclaredElementInstance candidate)
  {
    return candidate.Element.CreateDeclaredElementOccurrence();
  }
}

