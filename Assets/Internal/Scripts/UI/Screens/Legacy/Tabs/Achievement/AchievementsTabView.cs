using System.Collections.Generic;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Core.Tabs;
using Internal.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Achievement
{
    public sealed class AchievementsTabView : MonoBehaviour, ITabView
    {
        [SerializeField] private AchievementEntryView _entryPrefab;
        [SerializeField] private Transform _container;

        [Header("Footer")]
        [SerializeField] private TextMeshProUGUI _totalCounterLabel;

        private readonly List<AchievementEntryView> _entries = new();
        private LegacyAchievementsTabViewModel _viewModel;
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _counterHandle;
        private LocalizedString _counterLocalized;
        private int _earnedCount;
        private int _totalCount;

        private void Awake()
        {
            _counterLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Achievement_TotalCounter);
        }

        public int TabId => (int)LegacyTab.Achievements;

        public void Bind(LegacyAchievementsTabViewModel viewModel, LocalizationService localization)
        {
            _viewModel = viewModel;
            _localization = localization;
            RebindCounter();
        }

        public void BindLocalization(LocalizationService localization)
        {
            _localization = localization;
            RebindCounter();
        }

        public void DisposeBinding()
        {
            _counterHandle?.Dispose();
            _counterHandle = null;
        }

        public void Apply(IReadOnlyList<AchievementEntryData> achievements)
        {
            foreach (var entry in _entries)
            {
                if (entry == null) continue;
                entry.DisposeBinding();
                Destroy(entry.gameObject);
            }
            _entries.Clear();

            int earned = 0;
            foreach (AchievementEntryData data in achievements)
            {
                AchievementEntryView entry = Instantiate(_entryPrefab, _container);
                entry.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                if (_localization != null) entry.BindLocalization(_localization);
                entry.Apply(data);
                _entries.Add(entry);
                if (data.IsEarned) earned++;
            }

            _earnedCount = earned;
            _totalCount = achievements.Count;
            RebindCounter();
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Apply(object state)
        {
            if (state is LegacyAchievementsTabState achievementsState)
                Apply(achievementsState.Achievements);
        }

        private void RebindCounter()
        {
            if (_totalCounterLabel == null || _localization == null) return;
            _counterHandle?.Dispose();
            _counterHandle = _localization.BindText(
                _totalCounterLabel,
                _counterLocalized,
                $"{name}.Counter",
                $"{_earnedCount}/{_totalCount}",
                null,
                _earnedCount, _totalCount);
        }

        private void OnDestroy() => DisposeBinding();
    }
}
