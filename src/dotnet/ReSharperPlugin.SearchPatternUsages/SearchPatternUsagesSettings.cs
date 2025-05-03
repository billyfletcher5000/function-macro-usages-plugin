using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Options;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Resources.Settings;
using JetBrains.Threading;
using JetBrains.Util.Logging;

namespace ReSharperPlugin.SearchPatternUsages
{
    [SettingsKey(typeof(CodeInspectionSettings), "Search Pattern Usages Plugin Settings")]
    public class SearchPatternUsagesSettings
    {
        [SettingsEntry(SearchPatternDefaultValue, "Search Patterns")]
        public string SearchPatterns;

        private const string SearchPatternDefaultValue = "<SearchPatternUsagesSearchEntrySettings>\r\n  <SearchPatternUsagesSearchEntrySetting SearchPattern=\"Get{Foo}\" SearchFunctions=\"True\" SearchFunctionTemplates=\"True\" SearchVariables=\"False\" SearchVariableTemplates=\"False\" SearchTypeAliases=\"False\" SearchTypeAliasTemplates=\"False\" />\r\n  <SearchPatternUsagesSearchEntrySetting SearchPattern=\"GetRef{Foo}\" SearchFunctions=\"True\" SearchFunctionTemplates=\"True\" SearchVariables=\"False\" SearchVariableTemplates=\"False\" SearchTypeAliases=\"False\" SearchTypeAliasTemplates=\"False\" />\r\n  <SearchPatternUsagesSearchEntrySetting SearchPattern=\"Set{Foo}\" SearchFunctions=\"True\" SearchFunctionTemplates=\"True\" SearchVariables=\"False\" SearchVariableTemplates=\"False\" SearchTypeAliases=\"False\" SearchTypeAliasTemplates=\"False\" />\r\n  <SearchPatternUsagesSearchEntrySetting SearchPattern=\"Update{Foo}\" SearchFunctions=\"True\" SearchFunctionTemplates=\"True\" SearchVariables=\"False\" SearchVariableTemplates=\"False\" SearchTypeAliases=\"False\" SearchTypeAliasTemplates=\"False\" />\r\n</SearchPatternUsagesSearchEntrySettings>";
    }

    public class SearchPatternUsagesSearchEntrySetting
    {
        public readonly string SearchPattern;
        public readonly bool SearchFunctions;
        public readonly bool SearchFunctionTemplates;
        public readonly bool SearchTypeAliases;
        public readonly bool SearchTypeAliasTemplates;
        public readonly bool SearchVariables;
        public readonly bool SearchVariableTemplates;

        public SearchPatternUsagesSearchEntrySetting(string searchPattern, 
            bool searchFunctions, bool searchFunctionTemplates, 
            bool searchVariables, bool searchVariableTemplates, 
            bool searchTypeAliases, bool searchTypeAliasTemplates)
        {
            SearchPattern = searchPattern;
            SearchFunctions = searchFunctions;
            SearchFunctionTemplates = searchFunctionTemplates;
            SearchVariables = searchVariables;
            SearchVariableTemplates = searchVariableTemplates;
            SearchTypeAliases = searchTypeAliases;
            SearchTypeAliasTemplates = searchTypeAliasTemplates;
        }

        public override bool Equals(object obj)
        {
            return obj is SearchPatternUsagesSearchEntrySetting searchEntrySetting
                   && SearchPattern.Equals(searchEntrySetting.SearchPattern)
                   && SearchFunctions.Equals(searchEntrySetting.SearchFunctions)
                   && SearchFunctionTemplates.Equals(searchEntrySetting.SearchFunctionTemplates)
                   && SearchVariables.Equals(searchEntrySetting.SearchVariables)
                   && SearchVariableTemplates.Equals(searchEntrySetting.SearchVariableTemplates)
                   && SearchTypeAliases.Equals(searchEntrySetting.SearchTypeAliases)
                   && SearchTypeAliasTemplates.Equals(searchEntrySetting.SearchTypeAliasTemplates);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SearchPattern != null ? SearchPattern.GetHashCode() : 0;
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

    public class SearchPatternUsagesModel
    {
        private readonly Lifetime _lifetime;

        [NotNull] private readonly GroupingEvent _saveRequested;

        [NotNull] private readonly OptionsSettingsSmartContext _smartContext;

        public SearchPatternUsagesModel(
            Lifetime lifetime,
            [NotNull] OptionsSettingsSmartContext smartContext,
            IThreading threading,
            string eventId = "SearchPatternUsagesOptionsPage.Event")
        {
            _lifetime = lifetime;
            _smartContext = smartContext;
            _saveRequested = threading.GroupingEvents[Rgc.Invariant]
                .CreateEvent(lifetime, eventId, TimeSpan.FromMilliseconds(100.0), Save);
            Entries = new ListEvents<SearchEntry>("SearchPatternUsagesModel.Entries");
            SelectedEntry = new Property<SearchEntry>("SearchPatternUsagesModel.SelectedEntry");
            Reset();
        }

        public ListEvents<SearchEntry> Entries { get; }

        public IProperty<SearchEntry> SelectedEntry { get; }

        public void Reset()
        {
            var lifetime = _lifetime;
            Entries.Clear();
            var list = SearchPatternUsagesSearchEntrySettingKeyAccessor.GetSearchEntrySettings(_smartContext);
            foreach (SearchPatternUsagesSearchEntrySetting entrySetting in list)
            {
                Entries.Add(new SearchEntry(lifetime,
                    _saveRequested.Incoming,
                    entrySetting.SearchPattern,
                    entrySetting.SearchFunctions,
                    entrySetting.SearchFunctionTemplates,
                    entrySetting.SearchVariables,
                    entrySetting.SearchVariableTemplates,
                    entrySetting.SearchTypeAliases,
                    entrySetting.SearchTypeAliasTemplates));
            }

            SelectedEntry.Value = null;
            Entries.AddRemove.Advise_NoAcknowledgement(lifetime, _saveRequested.Incoming.Fire);
        }
        
        public SearchEntry GetNewSearchEntry(int index)
        {
            return new SearchEntry(_lifetime, _saveRequested.Incoming);
        }

        public bool CanBeRemoved(int index)
        {
            if (index < 0 || index >= Entries.Count)
                return false;

            return true;
        }

        public bool CanMoveUp(int index)
        {
            return index > 0;
        }

        public bool CanMoveDown(int index)
        {
            return index < Entries.Count - 1;
        }

        private void Save()
        {
            List<SearchPatternUsagesSearchEntrySetting> entrySettings =
                new List<SearchPatternUsagesSearchEntrySetting>();
            foreach (SearchEntry entry in Entries)
            {
                entrySettings.Add(new SearchPatternUsagesSearchEntrySetting(entry.SearchPattern.Value,
                    entry.SearchFunctions.Value,
                    entry.SearchFunctionTemplates.Value, entry.SearchVariables.Value,
                    entry.SearchVariableTemplates.Value,
                    entry.SearchTypeAliases.Value, entry.SearchTypeAliasTemplates.Value));
            }

            SearchPatternUsagesSearchEntrySettingKeyAccessor.SetSearchEntrySettings(_smartContext, entrySettings);
        }

        public class SearchEntry
        {
            public SearchEntry(
                Lifetime lifetime,
                [NotNull] ISimpleSignal saveRequested,
                string searchPattern = "",
                bool searchFunctions = true,
                bool searchFunctionTemplates = true,
                bool searchVariables = false,
                bool searchVariableTemplates = false,
                bool searchTypeAliases = false,
                bool searchTypeAliasTemplates = false)
            {
                SearchPattern = new Property<string>("SearchEntry.SearchPattern", searchPattern ?? "");
                SearchPattern.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
                SearchFunctions = new Property<bool>("SearchEntry.SearchFunctions", searchFunctions);
                SearchFunctions.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
                SearchFunctionTemplates =
                    new Property<bool>("SearchEntry.SearchFunctionTemplates", searchFunctionTemplates);
                SearchFunctionTemplates.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
                SearchVariables = new Property<bool>("SearchEntry.SearchVariables", searchVariables);
                SearchVariables.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
                SearchVariableTemplates =
                    new Property<bool>("SearchEntry.SearchVariableTemplates", searchVariableTemplates);
                SearchVariableTemplates.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
                SearchTypeAliases = new Property<bool>("SearchEntry.SearchTypeAliases", searchTypeAliases);
                SearchTypeAliases.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
                SearchTypeAliasTemplates =
                    new Property<bool>("SearchEntry.SearchTypeAliasTemplates", searchTypeAliasTemplates);
                SearchTypeAliasTemplates.Change.Advise_NoAcknowledgement(lifetime, saveRequested.Fire);
            }

            public IProperty<string> SearchPattern { get; }
            public IProperty<bool> SearchFunctions { get; }
            public IProperty<bool> SearchFunctionTemplates { get; }
            public IProperty<bool> SearchVariables { get; }
            public IProperty<bool> SearchVariableTemplates { get; }
            public IProperty<bool> SearchTypeAliases { get; }
            public IProperty<bool> SearchTypeAliasTemplates { get; }
        }
    }

    public class SearchPatternUsagesSettingUtil
    {
        private const string SearchPattern = "SearchPattern";
        private const string SearchFunctions = "SearchFunctions";
        private const string SearchFunctionTemplates = "SearchFunctionTemplates";
        private const string SearchVariables = "SearchVariables";
        private const string SearchVariableTemplates = "SearchVariableTemplates";
        private const string SearchTypeAliases = "SearchTypeAliases";
        private const string SearchTypeAliasTemplates = "SearchTypeAliasTemplates";

        public const string RegexPattern = "(?i){Foo}";

        public static XElement SettingToXml(SearchPatternUsagesSearchEntrySetting setting)
        {
            return new XElement((XName)"SearchPatternUsagesSearchEntrySetting", new object[7]
            {
                (object)new XAttribute((XName)SearchPattern, (object)setting.SearchPattern),
                (object)new XAttribute((XName)SearchFunctions, (object)setting.SearchFunctions.ToString()),
                (object)new XAttribute((XName)SearchFunctionTemplates,
                    (object)setting.SearchFunctionTemplates.ToString()),
                (object)new XAttribute((XName)SearchVariables, (object)setting.SearchVariables.ToString()),
                (object)new XAttribute((XName)SearchVariableTemplates,
                    (object)setting.SearchVariableTemplates.ToString()),
                (object)new XAttribute((XName)SearchTypeAliases, (object)setting.SearchTypeAliases.ToString()),
                (object)new XAttribute((XName)SearchTypeAliasTemplates,
                    (object)setting.SearchTypeAliasTemplates.ToString())
            });
        }

        public static SearchPatternUsagesSearchEntrySetting XmlToSetting(XElement element)
        {
            try
            {
                if (element == null)
                    return (SearchPatternUsagesSearchEntrySetting)null;

                var searchPattern = element.Attribute((XName)SearchPattern)?.Value;
                if (searchPattern == null)
                    throw new ArgumentException(SearchPattern);

                var searchFunctions = element.Attribute((XName)SearchFunctions)?.Value;
                if (searchFunctions == null)
                    throw new ArgumentException(SearchFunctions);

                var searchFunctionTemplates = element.Attribute((XName)SearchFunctionTemplates)?.Value;
                if (searchFunctionTemplates == null)
                    throw new ArgumentException(SearchFunctionTemplates);

                var searchVariables = element.Attribute((XName)SearchVariables)?.Value;
                if (searchVariables == null)
                    throw new ArgumentException(SearchVariables);

                var searchVariableTemplates = element.Attribute((XName)SearchVariableTemplates)?.Value;
                if (searchVariableTemplates == null)
                    throw new ArgumentException(SearchVariableTemplates);

                var searchTypeAliases = element.Attribute((XName)SearchTypeAliases)?.Value;
                if (searchTypeAliases == null)
                    throw new ArgumentException(SearchTypeAliases);

                var searchTypeAliasTemplates = element.Attribute((XName)SearchTypeAliasTemplates)?.Value;
                if (searchTypeAliasTemplates == null)
                    throw new ArgumentException(SearchTypeAliasTemplates);

                return new SearchPatternUsagesSearchEntrySetting(searchPattern,
                    bool.Parse(searchFunctions),
                    bool.Parse(searchFunctionTemplates),
                    bool.Parse(searchVariables),
                    bool.Parse(searchVariableTemplates),
                    bool.Parse(searchTypeAliases),
                    bool.Parse(searchTypeAliasTemplates));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return (SearchPatternUsagesSearchEntrySetting)null;
            }
        }

        public static string SearchPatternUsagesSearchEntrySettingsToString(
            IEnumerable<SearchPatternUsagesSearchEntrySetting> settings)
        {
            var xelement = new XElement((XName)"SearchPatternUsagesSearchEntrySettings");
            foreach (var setting in settings)
                xelement.Add((object)SettingToXml(setting));
            return xelement.ToString();
        }

        public static IEnumerable<SearchPatternUsagesSearchEntrySetting> StringToSearchPatternUsagesSearchEntrySettings(
            string value)
        {
            var searchEntries = new List<SearchPatternUsagesSearchEntrySetting>();
            if (string.IsNullOrEmpty(value))
                return (IEnumerable<SearchPatternUsagesSearchEntrySetting>)searchEntries;
            try
            {
                foreach (var element in XElement.Parse(value).Elements())
                {
                    var setting = XmlToSetting(element);
                    if (setting != null)
                        searchEntries.Add(setting);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return (IEnumerable<SearchPatternUsagesSearchEntrySetting>)searchEntries;
        }
    }

    public static class SearchPatternUsagesSearchEntrySettingKeyAccessor
    {
        public static IEnumerable<SearchPatternUsagesSearchEntrySetting> GetSearchEntrySettings(
            IContextBoundSettingsStore store)
        {
            return SearchPatternUsagesSettingUtil.StringToSearchPatternUsagesSearchEntrySettings(
                store.GetValue<SearchPatternUsagesSettings, string>(
                    (Expression<Func<SearchPatternUsagesSettings, string>>)(key => key.SearchPatterns)));
        }

        public static void SetSearchEntrySettings(
            IContextBoundSettingsStore store,
            IEnumerable<SearchPatternUsagesSearchEntrySetting> categories)
        {
            string str = SearchPatternUsagesSettingUtil.SearchPatternUsagesSearchEntrySettingsToString(categories);
            store.SetValue<SearchPatternUsagesSettings, string>(
                (Expression<Func<SearchPatternUsagesSettings, string>>)(key => key.SearchPatterns), str);
        }
    }

    public static class Strings
    {
        public static class ColumnText
        {
            public const string SearchPattern = "Search Pattern,1.5*";
            public const string SearchFunctions = "Functions,1*";
            public const string SearchFunctionTemplates = "Function Templates,1*";
            public const string SearchVariables = "Variables,1*";
            public const string SearchVariableTemplates = "Variable Templates,1*";
            public const string SearchTypeAliases = "Type Aliases,1*";
            public const string SearchTypeAliasTemplates = "Type Alias Templates,1*";
        }
    }
}