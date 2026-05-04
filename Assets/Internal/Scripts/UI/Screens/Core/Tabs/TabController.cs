using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Core.Tabs
{
    public sealed class TabController
    {
        private readonly ITabBinding[] _bindings;
        private readonly Action<int> _onSelected;
        private readonly UiThemeService _uiThemeService;

        private int _activeTab;
        private bool _hasActiveTab;

        public TabController(ITabBinding[] bindings, Action<int> onSelected, UiThemeService uiThemeService)
        {
            _bindings = bindings ?? Array.Empty<ITabBinding>();
            _onSelected = onSelected;
            _uiThemeService = uiThemeService;
        }

        public int ActiveTab => _activeTab;
        public bool HasActiveTab => _hasActiveTab;

        public bool TryGetDefaultTab(out int tabId)
        {
            foreach (ITabBinding binding in _bindings)
            {
                if (binding == null) continue;
                tabId = binding.TabId;
                return true;
            }

            tabId = default;
            return false;
        }

        public void BindLocalization(LocalizationService localization, string context)
        {
            for (int i = 0; i < _bindings.Length; i++)
            {
                ITabBinding binding = _bindings[i];
                binding?.Button?.BindLabel(localization, binding.Label, $"{context}.Tab{i}");
            }

            foreach (ITabView view in GetViews())
                view.BindLocalization(localization);
        }

        public void Subscribe()
        {
            foreach (ITabBinding binding in _bindings)
                binding?.Button?.AddListener(Select);
        }

        public void Unsubscribe()
        {
            foreach (ITabBinding binding in _bindings)
                binding?.Button?.RemoveListener();
        }

        public void DisposeBinding()
        {
            foreach (ITabBinding binding in _bindings)
                binding?.Button?.DisposeBinding();

            foreach (ITabView view in GetViews())
                view.DisposeBinding();
        }

        public void Select(int tabId)
        {
            _activeTab = tabId;
            _hasActiveTab = true;
            ApplyActiveState();
            _onSelected?.Invoke(tabId);
        }

        public void Apply(int tabId, object state)
        {
            _activeTab = tabId;
            _hasActiveTab = true;
            ApplyActiveState();

            foreach (ITabView view in GetViews())
            {
                if (view.TabId != tabId) continue;
                view.Apply(state);
                break;
            }
        }

        private void Select(TabButton button)
        {
            foreach (ITabBinding binding in _bindings)
            {
                if (binding?.Button != button) continue;
                Select(binding.TabId);
                return;
            }
        }

        private void ApplyActiveState()
        {
            foreach (ITabBinding binding in _bindings)
            {
                if (binding?.Button == null) continue;
                bool isSelected = _activeTab == binding.TabId;
                binding.Button.SetSelected(isSelected, GetButtonColor(isSelected));
            }

            foreach (ITabView view in GetViews())
                view.SetActive(view.TabId == _activeTab);
        }

        private (Color, Color) GetButtonColor(bool isSelected)
        {
            if (isSelected)
            {
                return (_uiThemeService.GetColor(ColorSlot.WindowContainer), _uiThemeService.GetColor(ColorSlot.WindowContainerText));
            }

            return (_uiThemeService.GetColor(ColorSlot.WindowElement), _uiThemeService.GetColor(ColorSlot.WindowElementText));
        }

        private ITabView[] GetViews()
        {
            var views = new ITabView[_bindings.Length];
            int count = 0;

            foreach (ITabBinding binding in _bindings)
            {
                ITabView view = binding?.View;
                if (view == null) continue;
                views[count++] = view;
            }

            if (count == views.Length) return views;

            Array.Resize(ref views, count);
            return views;
        }
    }
}
