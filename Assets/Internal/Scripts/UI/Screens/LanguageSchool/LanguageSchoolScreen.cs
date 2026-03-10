using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.LanguageSchool
{
    public class LanguageSchoolScreen : PopupScreen
    {
        [SerializeField] private LanguageSchoolItemView _itemPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TextMeshProUGUI _attentionText;
        [SerializeField] private TextMeshProUGUI _additionalHeaderText;
        [SerializeField] private ResourceIndicator _moneyIndicator;
        [SerializeField] private ResourceIndicator _foodIndicator;

        [Header("Localization")]
        [SerializeField] private LocalizedString _attentionLocalizedString;
        [SerializeField] private LocalizedString _availableLanguagesLocalizedString;

        private LanguageSchoolScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private readonly List<LanguageSchoolItemView> _spawnedItems = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            SetStaticTexts();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            base.OnDisable();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as LanguageSchoolScreenViewModel;
            SetupIcons();
            SubscribeViewModel();
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _moneyIndicator?.SetIcon(icons.Get(ResourceType.Money)?.Icon);
            _foodIndicator?.SetIcon(icons.Get(ResourceType.Food)?.Icon);
        }

        private void SetStaticTexts()
        {
            if (_additionalHeaderText != null && _availableLanguagesLocalizedString != null)
            {
                _additionalHeaderText.text = LocalizationService.ResolveString(
                    _availableLanguagesLocalizedString, "Available Languages", "LanguageSchool.Header");
            }

            if (_attentionText != null && _attentionLocalizedString != null)
            {
                _attentionText.text = LocalizationService.ResolveString(
                    _attentionLocalizedString, "Training requires spending days in the city",
                    "LanguageSchool.Attention");
            }
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(LanguageSchoolViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);
            _foodIndicator?.SetValue(state.PlayerFood);
            RebuildItems(state.Entries);
        }

        private void RebuildItems(IReadOnlyList<LanguageSchoolEntry> entries)
        {
            foreach (var item in _spawnedItems)
                Destroy(item.gameObject);
            _spawnedItems.Clear();

            if (entries == null || _itemPrefab == null || _content == null)
                return;

            foreach (var entry in entries)
            {
                var instance = Instantiate(_itemPrefab, _content);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(entry, HandleLearnClicked);
                _spawnedItems.Add(instance);
            }
        }

        private void HandleLearnClicked(LanguageType language)
        {
            _viewModel?.LearnLanguage(language);
        }
    }
}
