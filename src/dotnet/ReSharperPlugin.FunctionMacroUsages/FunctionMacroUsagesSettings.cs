using JetBrains.Application.Settings;
using JetBrains.ReSharper.Resources.Settings;

namespace ReSharperPlugin.ContextActions;

[SettingsKey(
    Parent: typeof(CodeInspectionSettings),
    Description: "Function Macro Usages Plugin Settings")]
public class FunctionMacroUsagesSettings
{
    [SettingsEntry("","Macro Search Patterns")]
    public IIndexedEntry<GuidIndex, FunctionMacroUsagesSearchEntrySettings> MacroSearchPatterns;
}

public class FunctionMacroUsagesSearchEntrySettings
{
    public readonly string SearchPattern;
    public readonly bool SearchFunctions = true;
    public readonly bool SearchFunctionTemplates = true;
    public readonly bool SearchVariables = false;
    public readonly bool SearchVariableTemplates = false;
    public readonly bool SearchTypeAliases = false;
    public readonly bool SearchTypeAliasTemplates = false;

    public FunctionMacroUsagesSearchEntrySettings(string searchPattern, bool searchFunctions, bool searchFunctionTemplates, bool searchVariables, bool searchVariableTemplates, bool searchTypeAliases, bool searchTypeAliasTemplates)
    {
        this.SearchPattern = searchPattern;
        this.SearchFunctions = searchFunctions;
        this.SearchFunctionTemplates = searchFunctionTemplates;
        this.SearchVariables = searchVariables;
        this.SearchVariableTemplates = searchVariableTemplates;
        this.SearchTypeAliases = searchTypeAliases;
        this.SearchTypeAliasTemplates = searchTypeAliasTemplates;
    }

    public override bool Equals(object obj)
    {
        return obj is FunctionMacroUsagesSearchEntrySettings includeCategorySetting
               && this.SearchPattern.Equals(includeCategorySetting.SearchPattern)
               && this.SearchFunctions.Equals(includeCategorySetting.SearchFunctions)
               && this.SearchFunctionTemplates.Equals(includeCategorySetting.SearchFunctionTemplates)
               && this.SearchVariables.Equals(includeCategorySetting.SearchVariables)
               && this.SearchVariableTemplates.Equals(includeCategorySetting.SearchVariableTemplates)
               && this.SearchTypeAliases.Equals(includeCategorySetting.SearchTypeAliases)
               && this.SearchTypeAliasTemplates.Equals(includeCategorySetting.SearchTypeAliasTemplates);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (SearchPattern != null ? SearchPattern.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ SearchFunctions.GetHashCode();
            hashCode = (hashCode * 397) ^ SearchFunctionTemplates.GetHashCode();
            hashCode = (hashCode * 397) ^ SearchVariables.GetHashCode();
            hashCode = (hashCode * 397) ^ SearchVariableTemplates.GetHashCode();
            hashCode = (hashCode * 397) ^ SearchTypeAliases.GetHashCode();
            hashCode = (hashCode * 397) ^ SearchTypeAliasTemplates.GetHashCode();
            return hashCode;
        }
    }
}

