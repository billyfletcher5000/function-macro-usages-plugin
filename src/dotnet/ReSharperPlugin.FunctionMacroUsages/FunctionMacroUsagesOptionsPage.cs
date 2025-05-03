
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.Core;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.IDE.UI.Extensions.Validation;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Cpp.CodeStyle.IncludesOrder;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.Rider.Model;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.ReSharper.Feature.Services.Cpp.Options;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;
using JetBrains.ReSharper.Feature.Services.UI.Validation;
using JetBrains.ReSharper.UnitTestFramework.Resources;

#nullable disable
namespace ReSharperPlugin.FunctionMacroUsages
{
  [OptionsPage(FunctionMacroUsagesOptionsPage.PID, "Function Macro Usages", typeof (UnitTestingThemedIcons.Session), ParentId = CppOptionsPage.PID)]
  public class FunctionMacroUsagesOptionsPage : BeSimpleOptionsPage
  {
    private const string PID = "FunctionMacroUsagesOptions";
    private static readonly Func<string, bool> ourValidateRegex = (Func<string, bool>) (pattern =>
    {
      try
      {
        Regex regex = new Regex("(?i){Foo}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        return regex.Match(pattern).Success;
      }
      catch (Exception)
      {
        return false;
      }
    });

    public FunctionMacroUsagesOptionsPage(
      Lifetime lifetime,
      OptionsPageContext optionsPageContext,
      OptionsSettingsSmartContext smartContext,
      IconHostBase iconHost,
      HighlightingSettingsManager manager,
      IThreading threading)
      : base(lifetime, optionsPageContext, smartContext, true)
    {
      AddHeader("Search Patterns");
      AddControl(GetSearchEntryTable(lifetime, smartContext, iconHost, threading).WithDescription("Search Entries", lifetime, GridOrientation.Vertical), true);
    }

    private BeControl GetSearchEntryTable(
      Lifetime lifetime,
      OptionsSettingsSmartContext smartContext,
      IconHostBase iconHost,
      IThreading threading)
    {
      BeMargin margin = BeMargins.Create((BeMarginType.OnePx, 6), (BeMarginType.OnePx, 4), (BeMarginType.None, 0), (BeMarginType.OnePx, 2)); 
      FunctionMacroUsagesModel model = new FunctionMacroUsagesModel(lifetime, smartContext, threading);
      BeToolbar selectionListWithToolbar = model.SelectedEntry.GetBeSingleSelectionListWithToolbar<FunctionMacroUsagesModel.SearchEntry>((IListEvents<FunctionMacroUsagesModel.SearchEntry>) model.Entries, lifetime, (PresentListLine<FunctionMacroUsagesModel.SearchEntry>) ((entryLt, entry, properties) =>
      {
            return new List<BeControl>()
            {
              (BeControl) entry.SearchPattern.GetBeTextBox(entryLt).WithValidationRule<BeTextBox, string>(entryLt, FunctionMacroUsagesOptionsPage.ourValidateRegex, "Missing {Foo} placeholder in search pattern!").WithTextNotEmpty<BeTextBox>(entryLt, (IconModel) null),
              (BeControl) entry.SearchFunctions.GetBeCheckBox(entryLt, ""),
              (BeControl) entry.SearchFunctionTemplates.GetBeCheckBox(entryLt, ""),
              (BeControl) entry.SearchVariables.GetBeCheckBox(entryLt, ""),
              (BeControl) entry.SearchVariableTemplates.GetBeCheckBox(entryLt, ""),
              (BeControl) entry.SearchTypeAliases.GetBeCheckBox(entryLt, ""),
              (BeControl) entry.SearchTypeAliasTemplates.GetBeCheckBox(entryLt, ""),
            };
      }), (IIconHost) iconHost, new string[7]
      {
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchPattern_Text,
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchFunctions_Text,
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchFunctionTemplates_Text,
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchVariables_Text,
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchVariableTemplates_Text,
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchTypeAliases_Text,
        ReSharperPlugin.FunctionMacroUsages.Strings.SearchTypeAliasTemplates_Text
      });
      this.Reload.Advise<Unit>(lifetime, (Action) (() => model.Reset()));
      Func<int, FunctionMacroUsagesModel.SearchEntry> getNewElement = (Func<int, FunctionMacroUsagesModel.SearchEntry>) (i => model.GetNewSearchEntry(i));
      string addPatternText = "Add Search Pattern";
      return (BeControl) selectionListWithToolbar
          .AddButtonWithListAction<FunctionMacroUsagesModel.SearchEntry>(BeListAddAction.ADD, getNewElement, style: BeButtonStyle.DEFAULT, customTooltip: addPatternText)
          .AddButtonWithListAction<FunctionMacroUsagesModel.SearchEntry>(BeListAction.REMOVE, canExecute: (Func<int, bool>) (i => model.CanBeRemoved(i)), customTooltip: "Remove a search entry", style: BeButtonStyle.DEFAULT)
          .AddButtonWithListAction<FunctionMacroUsagesModel.SearchEntry>(BeListAction.MOVE_UP, canExecute: (Func<int, bool>) (i => model.CanMoveUp(i)), customTooltip: "Move a search entry up", style: BeButtonStyle.DEFAULT)
          .AddButtonWithListAction<FunctionMacroUsagesModel.SearchEntry>(BeListAction.MOVE_DOWN, canExecute: (Func<int, bool>) (i => model.CanMoveDown(i)), customTooltip: "Move a search entry down", style: BeButtonStyle.DEFAULT);
    }
  }
}
