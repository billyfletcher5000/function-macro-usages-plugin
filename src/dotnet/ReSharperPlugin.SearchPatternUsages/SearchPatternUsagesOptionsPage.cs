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
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.Rider.Model;
using JetBrains.Rider.Model.UIAutomation;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.ReSharper.Feature.Services.Cpp.Options;
using JetBrains.ReSharper.Feature.Services.UI.Validation;
using JetBrains.ReSharper.UnitTestFramework.Resources;

namespace ReSharperPlugin.SearchPatternUsages
{
    [OptionsPage(PID, "Search Pattern Usages", typeof(UnitTestingThemedIcons.Session), ParentId = ToolsPage.PID)]
    public class SearchPatternUsagesOptionsPage : BeSimpleOptionsPage
    {
        private const string PID = "SearchPatternUsagesOptions";

        private static readonly Func<string, bool> ValidationRegex = (Func<string, bool>)(pattern =>
        {
            try
            {
                var regex = new Regex(SearchPatternUsagesSettingUtil.RegexPattern,
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                return regex.Match(pattern).Success;
            }
            catch (Exception)
            {
                return false;
            }
        });

        public SearchPatternUsagesOptionsPage(
            Lifetime lifetime,
            OptionsPageContext optionsPageContext,
            OptionsSettingsSmartContext smartContext,
            IconHostBase iconHost,
            HighlightingSettingsManager manager,
            IThreading threading)
            : base(lifetime, optionsPageContext, smartContext, true)
        {
            AddText("Search patterns can be added that will look for functions, variables and/or type aliases in the same class/struct as the target of a Find Usages action.\n\nEach pattern should use \"{Foo}\" as a placeholder, it will be replaced by the Find Usage target's name, e.g. a pattern of \"Get{Foo}\" applied to a Find Usages search on a variable \"int Count\" will match to elements on the same class named GetCount.");
            AddSpacer();
            AddSpacer();
            AddSpacer();
            AddControl(
                GetSearchEntryTable(lifetime, smartContext, iconHost, threading)
                    .WithDescription("Search Entries", lifetime, GridOrientation.Vertical), true);
        }

        private BeControl GetSearchEntryTable(
            Lifetime lifetime,
            OptionsSettingsSmartContext smartContext,
            IconHostBase iconHost,
            IThreading threading)
        {
            var margin = BeMargins.Create((BeMarginType.OnePx, 6), (BeMarginType.OnePx, 4), (BeMarginType.None, 0),
                (BeMarginType.OnePx, 2));
            var model = new SearchPatternUsagesModel(lifetime, smartContext, threading);
            var selectionListWithToolbar =
                model.SelectedEntry.GetBeSingleSelectionListWithToolbar<SearchPatternUsagesModel.SearchEntry>(
                    (IListEvents<SearchPatternUsagesModel.SearchEntry>)model.Entries, lifetime,
                    (PresentListLine<SearchPatternUsagesModel.SearchEntry>)((entryLt, entry, properties) =>
                    {
                        return new List<BeControl>()
                        {
                            (BeControl)entry.SearchPattern.GetBeTextBox(entryLt)
                                .WithValidationRule<BeTextBox, string>(entryLt, ValidationRegex,
                                    "Missing {Foo} placeholder in search pattern!")
                                .WithTextNotEmpty<BeTextBox>(entryLt, (IconModel)null),
                            (BeControl)entry.SearchFunctions.GetBeCheckBox(entryLt, ""),
                            (BeControl)entry.SearchFunctionTemplates.GetBeCheckBox(entryLt, ""),
                            (BeControl)entry.SearchVariables.GetBeCheckBox(entryLt, ""),
                            (BeControl)entry.SearchVariableTemplates.GetBeCheckBox(entryLt, ""),
                            (BeControl)entry.SearchTypeAliases.GetBeCheckBox(entryLt, ""),
                            (BeControl)entry.SearchTypeAliasTemplates.GetBeCheckBox(entryLt, "")
                        };
                    }), (IIconHost)iconHost, new string[7]
                    {
                        Strings.ColumnText.SearchPattern,
                        Strings.ColumnText.SearchFunctions,
                        Strings.ColumnText.SearchFunctionTemplates,
                        Strings.ColumnText.SearchVariables,
                        Strings.ColumnText.SearchVariableTemplates,
                        Strings.ColumnText.SearchTypeAliases,
                        Strings.ColumnText.SearchTypeAliasTemplates
                    });
            Reload.Advise<Unit>(lifetime, (Action)(() => model.Reset()));
            var getNewElement = (Func<int, SearchPatternUsagesModel.SearchEntry>)(i => model.GetNewSearchEntry(i));
            var addPatternText = "Add Search Pattern";
            return (BeControl)selectionListWithToolbar
                .AddButtonWithListAction<SearchPatternUsagesModel.SearchEntry>(BeListAddAction.ADD, getNewElement,
                    style: BeButtonStyle.DEFAULT, customTooltip: addPatternText)
                .AddButtonWithListAction<SearchPatternUsagesModel.SearchEntry>(BeListAction.REMOVE,
                    canExecute: (Func<int, bool>)(i => model.CanBeRemoved(i)), customTooltip: "Remove a search entry",
                    style: BeButtonStyle.DEFAULT)
                .AddButtonWithListAction<SearchPatternUsagesModel.SearchEntry>(BeListAction.MOVE_UP,
                    canExecute: (Func<int, bool>)(i => model.CanMoveUp(i)), customTooltip: "Move a search entry up",
                    style: BeButtonStyle.DEFAULT)
                .AddButtonWithListAction<SearchPatternUsagesModel.SearchEntry>(BeListAction.MOVE_DOWN,
                    canExecute: (Func<int, bool>)(i => model.CanMoveDown(i)), customTooltip: "Move a search entry down",
                    style: BeButtonStyle.DEFAULT);
        }
    }
}