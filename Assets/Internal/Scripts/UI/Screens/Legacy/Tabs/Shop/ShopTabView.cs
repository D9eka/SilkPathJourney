using System;
using System.Collections.Generic;
using Internal.Scripts.Meta.Unlocks;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Core.Tabs;
using Internal.Scripts.UI.Theme;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Shop
{
    public sealed class ShopTabView : MonoBehaviour, ITabView
    {
        [SerializeField] private UnlockEntryView _entryPrefab;
        [SerializeField] private CategorySectionView _categorySectionPrefab;
        [SerializeField] private Transform _container;

        private readonly List<CategorySectionView> _sections = new();
        private readonly List<UnlockEntryView> _flatEntries = new();
        private LegacyShopTabViewModel _viewModel;
        private LocalizationService _localization;
        private Action<string> _onUnlock;

        public int TabId => (int)LegacyTab.Shop;

        public void Bind(LegacyShopTabViewModel viewModel, Action<string> onUnlock, LocalizationService localization)
        {
            _viewModel = viewModel;
            _onUnlock = onUnlock;
            _localization = localization;
        }

        public void BindLocalization(LocalizationService localization)
        {
            _localization = localization;
        }

        public void Apply(IReadOnlyList<UnlockEntryData> unlocks, int currentPoints, Action<string> onUnlock)
        {
            _onUnlock = onUnlock;
            foreach (var section in _sections)
            {
                if (section == null) continue;
                section.DisposeBinding();
                Destroy(section.gameObject);
            }
            _sections.Clear();

            foreach (var entry in _flatEntries)
                if (entry != null) Destroy(entry.gameObject);
            _flatEntries.Clear();

            if (_categorySectionPrefab == null)
            {
                ApplyFlat(unlocks, currentPoints, onUnlock);
                return;
            }

            var byCategory = new Dictionary<UnlockCategory, List<UnlockEntryData>>();
            foreach (var data in unlocks)
            {
                if (!byCategory.TryGetValue(data.Category, out var list))
                {
                    list = new List<UnlockEntryData>();
                    byCategory[data.Category] = list;
                }
                list.Add(data);
            }

            var categoryOrder = new[]
            {
                UnlockCategory.Background,
                UnlockCategory.StartItem,
                UnlockCategory.Route,
                UnlockCategory.CaravanUpgrade
            };

            foreach (var category in categoryOrder)
            {
                if (!byCategory.TryGetValue(category, out var items) || items.Count == 0) continue;

                string locKey = GetCategoryLocKey(category);
                var section = Instantiate(_categorySectionPrefab, _container);
                section.Apply(locKey, _entryPrefab, items, currentPoints, onUnlock, _localization, _viewModel?.ThemeService);
                _sections.Add(section);
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Apply(object state)
        {
            if (state is not LegacyShopTabState shopState) return;
            Apply(shopState.Unlocks, shopState.LegacyPointsTotal, _onUnlock);
        }

        public void DisposeBinding()
        {
            foreach (var section in _sections)
                section?.DisposeBinding();
        }

        private void ApplyFlat(IReadOnlyList<UnlockEntryData> unlocks, int currentPoints, Action<string> onUnlock)
        {
            foreach (var data in unlocks)
            {
                var id = data.Id;
                var entry = Instantiate(_entryPrefab, _container);
                entry.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                if (_localization != null) entry.BindLocalization(_localization);
                entry.Apply(data, currentPoints, () => onUnlock?.Invoke(id));
                _flatEntries.Add(entry);
            }
        }

        private static string GetCategoryLocKey(UnlockCategory category)
        {
            return category switch
            {
                UnlockCategory.Background => LocUI.UI_LegacyShop_Category_Backstories,
                UnlockCategory.StartItem => LocUI.UI_LegacyShop_Category_StarterItems,
                UnlockCategory.Route => LocUI.UI_LegacyShop_Category_RouteKnowledge,
                UnlockCategory.CaravanUpgrade => LocUI.UI_LegacyShop_Category_CaravanUpgrades,
                _ => string.Empty
            };
        }
    }
}
