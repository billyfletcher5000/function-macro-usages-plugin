using System;
using System.Collections.Generic;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Controls.FileSystem;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.IDE.UI.Extensions.Validation;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Cpp.Options;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;
using JetBrains.ReSharper.Feature.Services.UI.Validation;
using JetBrains.ReSharper.UnitTestFramework.Resources;
using JetBrains.Rider.Model;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;

namespace ReSharperPlugin.ContextActions;


[OptionsPage(PID, PageTitle, typeof(UnitTestingThemedIcons.Session),
    ParentId = CodeInspectionPage.PID)]
public class FunctionMacroUsagesOptionsPage : BeSimpleOptionsPage
{
    private const string PID = nameof(FunctionMacroUsagesOptionsPage);
    private const string PageTitle = "Function Macro Usages";

    private readonly Lifetime _lifetime;

    public FunctionMacroUsagesOptionsPage(Lifetime lifetime,
        OptionsPageContext optionsPageContext,
        OptionsSettingsSmartContext optionsSettingsSmartContext,
        IconHostBase iconHost,
        ICommonFileDialogs dialogs)
        : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
    {
        _lifetime = lifetime;

        // Add additional search keywords
        AddKeyword("Sample", "Example", "Preferences"); // TODO: only works for ReSharper?

        AddText("This is a sample options page that works likewise in ReSharper and Rider.");
        AddSpacer();
        AddText($"It allows to view and manipulate values in the {nameof(FunctionMacroUsagesSettings)} class.");
        AddCommentText("Values are saved in a .dotSettings file.");

        AddHeader("Basic Options");
        AddIntOption()
    }
    
    private BeControl GetIncludeCategoriesTable(
      Lifetime lifetime,
      OptionsSettingsSmartContext smartContext,
      IconHostBase iconHost,
      IThreading threading)
    {
      BeMargin margin = BeMargins.Create((BeMarginType.OnePx, 6), (BeMarginType.OnePx, 4), (BeMarginType.None, 0), (BeMarginType.OnePx, 2));
      IncludeCategoriesModel model = new IncludeCategoriesModel(lifetime, smartContext, threading);
      BeToolbar selectionListWithToolbar = model.SelectedEntry.GetBeSingleSelectionListWithToolbar<IncludeCategoriesModel.IEntry>((IListEvents<IncludeCategoriesModel.IEntry>) model.Entries, lifetime, (PresentListLine<IncludeCategoriesModel.IEntry>) ((entryLt, entry, properties) =>
      {
        switch (entry)
        {
          case IncludeCategoriesModel.SeparatorEntry _:
            return new List<BeControl>()
            {
              new JetBrains.UI.RichText.RichText(JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.BlankLine_Text, new TextStyle(JetFontStyles.Italic, JetRgbaColors.Gray)).GetBeRichText().WithMargin(margin),
              BeControls.GetSpacer().WithMargin(margin)
            };
          case IncludeCategoriesModel.IncludeCategoryEntry includeCategoryEntry2:
            return new List<BeControl>()
            {
              (BeControl) includeCategoryEntry2.Pattern.GetBeTextBox(entryLt).WithValidationRule<BeTextBox, string>(entryLt, CppIncludesOrderPage.ourValidateRegex, JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.InvalidRegularExpression_Text).WithTextNotEmpty<BeTextBox>(entryLt, (IconModel) null),
              (BeControl) includeCategoryEntry2.Description.GetBeTextBox(entryLt)
            };
          case IncludeCategoriesModel.SpecialHeaderEntry specialHeaderEntry2:
            return new List<BeControl>()
            {
              new JetBrains.UI.RichText.RichText(specialHeaderEntry2.Pattern.Value, new TextStyle(JetFontStyles.Italic, JetRgbaColors.Gray)).GetBeRichText().WithMargin(margin),
              new JetBrains.UI.RichText.RichText(specialHeaderEntry2.Description.Value, new TextStyle(JetFontStyles.Italic, JetRgbaColors.Gray)).GetBeRichText().WithMargin(margin)
            };
          default:
            throw new ArgumentException(entry.GetType().ToString());
        }
      }), (IIconHost) iconHost, new string[2]
      {
        JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.RegexPattern_Text,
        JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.Description_Text
      });
      this.Reload.Advise<Unit>(lifetime, (Action) (() => model.Reset()));
      Func<int, IncludeCategoriesModel.IEntry> getNewElement = (Func<int, IncludeCategoriesModel.IEntry>) (i => model.GetNewIncludeCategoryEntry(i));
      string addPatternText = JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings._AddPattern_Text;
      return (BeControl) selectionListWithToolbar.AddButtonWithListAction<IncludeCategoriesModel.IEntry>(BeListAddAction.ADD_AFTER_SELECTED, getNewElement, style: BeButtonStyle.DEFAULT, customTooltip: addPatternText).AddButtonWithListAction<IncludeCategoriesModel.IEntry>(BeListAddAction.ADD_AFTER_SELECTED, (Func<int, IncludeCategoriesModel.IEntry>) (i => model.GetNewSeparatorCategoryEntry(i)), style: BeButtonStyle.DEFAULT, customTooltip: JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.Add_blankLine_Text).AddButtonWithListAction<IncludeCategoriesModel.IEntry>(BeListAction.REMOVE, canExecute: (Func<int, bool>) (i => model.CanBeRemoved(i)), customTooltip: JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings._Remove_Text, style: BeButtonStyle.DEFAULT).AddButtonWithListAction<IncludeCategoriesModel.IEntry>(BeListAction.MOVE_UP, canExecute: (Func<int, bool>) (i => model.CanMoveUp(i)), customTooltip: JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.Move_Up_Text, style: BeButtonStyle.DEFAULT).AddButtonWithListAction<IncludeCategoriesModel.IEntry>(BeListAction.MOVE_DOWN, canExecute: (Func<int, bool>) (i => model.CanMoveDown(i)), customTooltip: JetBrains.ReSharper.Feature.Services.Cpp.Resources.Strings.Move_Down_Text, style: BeButtonStyle.DEFAULT);
    }
}