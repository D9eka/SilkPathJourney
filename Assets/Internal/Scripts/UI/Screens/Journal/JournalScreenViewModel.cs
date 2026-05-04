using System.Collections.Generic;
using Internal.Scripts.Journal;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Journal
{
    public sealed class JournalScreenViewModel : ScreenViewModelBase
    {
        private readonly JournalService _journalService;
        private readonly UiThemeService _themeService;
        private readonly JournalIconCatalog _iconCatalog;

        private readonly ReactiveProperty<JournalViewState> _state = new();

        public Observable<JournalViewState> State => _state;
        public UiThemeService ThemeService => _themeService;
        public JournalIconCatalog IconCatalog => _iconCatalog;

        public JournalScreenViewModel(JournalService journalService, UiThemeService themeService, JournalIconCatalog iconCatalog)
        {
            _journalService = journalService;
            _themeService = themeService;
            _iconCatalog = iconCatalog;
        }

        public override ScreenId Id => ScreenId.Journal;

        protected override void OnOpen(object args)
        {
            _journalService.OnEntryAdded += BuildState;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            BuildState();
        }

        protected override void OnClose()
        {
            _journalService.OnEntryAdded -= BuildState;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale _) => BuildState();

        private void BuildState()
        {
            var days = new List<JournalDayEntry>();
            foreach (int day in _journalService.GetDays())
            {
                var lines = new List<JournalLineData>();
                foreach (var entry in _journalService.GetEntriesForDay(day))
                {
                    string text = LocalizationService.Resolve("UI", entry.TitleKey, entry.TitleKey,
                        entry.Args != null && entry.Args.Length > 0 ? entry.Args : System.Array.Empty<object>());
                    lines.Add(new JournalLineData(text, entry.Type));
                }
                if (lines.Count > 0)
                    days.Add(new JournalDayEntry(day, lines));
            }
            _state.Value = new JournalViewState(days);
        }
    }

    public readonly struct JournalLineData
    {
        public readonly string Text;
        public readonly JournalEntryType Type;

        public JournalLineData(string text, JournalEntryType type)
        {
            Text = text;
            Type = type;
        }
    }

    public class JournalViewState
    {
        public IReadOnlyList<JournalDayEntry> Days { get; }

        public JournalViewState(IReadOnlyList<JournalDayEntry> days)
        {
            Days = days;
        }
    }

    public class JournalDayEntry
    {
        public int Day { get; }
        public IReadOnlyList<JournalLineData> Lines { get; }

        public JournalDayEntry(int day, IReadOnlyList<JournalLineData> lines)
        {
            Day = day;
            Lines = lines;
        }
    }
}
