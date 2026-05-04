using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Shop
{
    public sealed class CategorySectionView : MonoBehaviour
    {
        [SerializeField] private Image _categoryTitleBackground;
        [SerializeField] private TextMeshProUGUI _categoryTitleText;
        [SerializeField] private Transform _container;

        private readonly List<UnlockEntryView> _entries = new();
        private LocalizationService.LocalizedTextHandle _titleHandle;

        public void Apply(string locKey, UnlockEntryView entryPrefab, IEnumerable<UnlockEntryData> items,
            int currentPoints, Action<string> onUnlock, LocalizationService localization, UiThemeService themeService)
        {
            _titleHandle?.Dispose();
            _titleHandle = null;

            gameObject.InitializeColorBinders(themeService);
            if (_categoryTitleText != null && localization != null && !string.IsNullOrEmpty(locKey))
            {
                _titleHandle = localization.BindText(
                    _categoryTitleText,
                    new LocalizedString("UI", locKey),
                    $"{name}.{locKey}");
            }

            foreach (var entry in _entries)
            {
                if (entry == null) continue;
                entry.gameObject.SetActive(false);
                Destroy(entry.gameObject);
            }
            _entries.Clear();

            if (entryPrefab == null || _container == null) return;

            for (int i = _container.childCount - 1; i >= 0; i--)
            {
                var child = _container.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            foreach (var data in items)
            {
                var id = data.Id;
                var entry = Instantiate(entryPrefab, _container);
                entry.gameObject.InitializeColorBinders(themeService: themeService);
                if (localization != null) entry.BindLocalization(localization);
                entry.Apply(data, currentPoints, () => onUnlock?.Invoke(id));
                _entries.Add(entry);
            }
        }

        public void DisposeBinding()
        {
            _titleHandle?.Dispose();
            _titleHandle = null;
        }

        private void OnDestroy()
        {
            DisposeBinding();
        }
    }
}
