using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    public class TraderScreen : PopupScreen
    {
        [Header("Prefabs")]
        [SerializeField] private ProfileItem _profileItemPrefab;
        [SerializeField] private SkillView _skillViewPrefab;
        [SerializeField] private SkillView _languageViewPrefab;

        [Header("Containers")]
        [SerializeField] private RectTransform _profileContent;
        [SerializeField] private RectTransform _skillsContent;
        [SerializeField] private RectTransform _languagesContent;

        [Header("Section Headers")]
        [SerializeField] private TextMeshProUGUI _profileHeaderText;
        [SerializeField] private LocalizedString _profileHeaderLocalized;
        [SerializeField] private TextMeshProUGUI _skillsHeaderText;
        [SerializeField] private LocalizedString _skillsHeaderLocalized;
        [SerializeField] private TextMeshProUGUI _languagesHeaderText;
        [SerializeField] private LocalizedString _languagesHeaderLocalized;

        private TraderScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private readonly List<ProfileItem> _spawnedProfiles = new();
        private readonly List<SkillView> _spawnedSkills = new();
        private readonly List<SkillView> _spawnedLanguages = new();
        private LocalizationService.LocalizedTextHandle _profileHeaderHandle;
        private LocalizationService.LocalizedTextHandle _skillsHeaderHandle;
        private LocalizationService.LocalizedTextHandle _languagesHeaderHandle;

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            _profileHeaderHandle?.Dispose();
            _skillsHeaderHandle?.Dispose();
            _languagesHeaderHandle?.Dispose();
            base.OnDisable();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindSectionHeaders();
        }

        private void BindSectionHeaders()
        {
            _profileHeaderHandle?.Dispose();
            if (_profileHeaderText != null && _profileHeaderLocalized != null && Localization != null)
                _profileHeaderHandle = Localization.BindText(_profileHeaderText, _profileHeaderLocalized, "Trader.ProfileHeader");

            _skillsHeaderHandle?.Dispose();
            if (_skillsHeaderText != null && _skillsHeaderLocalized != null && Localization != null)
                _skillsHeaderHandle = Localization.BindText(_skillsHeaderText, _skillsHeaderLocalized, "Trader.SkillsHeader");

            _languagesHeaderHandle?.Dispose();
            if (_languagesHeaderText != null && _languagesHeaderLocalized != null && Localization != null)
                _languagesHeaderHandle = Localization.BindText(_languagesHeaderText, _languagesHeaderLocalized, "Trader.LanguagesHeader");
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as TraderScreenViewModel;
            SubscribeViewModel();
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

        private void ApplyState(TraderViewState state)
        {
            RebuildProfiles(state.ProfileItems);
            RebuildSkills(state.Skills);
            RebuildLanguages(state.Languages);
        }

        private void RebuildProfiles(IReadOnlyList<ProfileEntry> items)
        {
            foreach (ProfileItem item in _spawnedProfiles)
                Destroy(item.gameObject);
            _spawnedProfiles.Clear();

            if (items == null || _profileItemPrefab == null || _profileContent == null)
                return;

            foreach (ProfileEntry entry in items)
            {
                ProfileItem instance = Instantiate(_profileItemPrefab, _profileContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(Localization, entry.Header, entry.Content);
                _spawnedProfiles.Add(instance);
            }
        }


        private void RebuildSkills(IReadOnlyList<SkillViewData> skills)
        {
            foreach (SkillView view in _spawnedSkills)
                Destroy(view.gameObject);
            _spawnedSkills.Clear();

            if (skills == null || _skillViewPrefab == null || _skillsContent == null)
                return;

            foreach (SkillViewData data in skills)
            {
                SkillView instance = Instantiate(_skillViewPrefab, _skillsContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(Localization, data.Name, data.Description, data.Progress, data.Value);
                _spawnedSkills.Add(instance);
            }
        }

        private void RebuildLanguages(IReadOnlyList<LanguageViewData> languages)
        {
            foreach (SkillView view in _spawnedLanguages)
                Destroy(view.gameObject);
            _spawnedLanguages.Clear();

            if (languages == null || _languageViewPrefab == null || _languagesContent == null)
                return;

            foreach (LanguageViewData data in languages)
            {
                SkillView instance = Instantiate(_languageViewPrefab, _languagesContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(Localization, data.Name, data.Description, data.Progress, data.Value);
                _spawnedLanguages.Add(instance);
            }
        }
    }
}
