using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Event
{
    public class EventScreen : ScreenViewBase
    {
        [Header("Header")]
        [SerializeField] private HeaderElement _mainHeader;
        [SerializeField] private LocalizedString _mainHeaderLocalizedString;
        [Header("Event Info")]
        [SerializeField] private TextMeshProUGUI _eventNameText;
        [SerializeField] private TextMeshProUGUI _eventTypeText;
        [SerializeField] private TextMeshProUGUI _eventDescriptionText;
        [SerializeField] private TextMeshProUGUI _eventLocationText;
        [SerializeField] private Image _eventImage;
        [Header("Resource Preview")]
        [SerializeField] private Transform _resourceIndicatorsRoot;
        [SerializeField] private ResourceIndicator _resourceIndicatorPrefab;
        [Header("Choices")]
        [SerializeField] private Transform _choiceButtonsRoot;
        [SerializeField] private EventChoiceButton _choiceButtonPrefab;
        [Header("Layout")]
        [SerializeField] private AdaptiveLayoutHeight _scrollViewLayout;
        [SerializeField] private AdaptiveScrollRect _scrollRect;
        [Header("Result")]
        [SerializeField] private LocalizedString _continueLocalizedString;
        [SerializeField] private LocalizedString _nearCityFormat;

        private EventScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _locationSubscription;
        private IDisposable _resultSubscription;
        private readonly List<EventChoiceButton> _activeButtons = new();
        private readonly List<ResourceIndicator> _spawnedIndicators = new();
        private readonly Dictionary<EventOutcomeType, ResourceIndicator> _resourceIndicators = new();
        private List<EventChoice> _currentChoices;
        private int _lastLinkIndex = -1;
        private string _unknownLanguageTooltip;

        private LocalizationService.LocalizedTextHandle _mainHeaderHandle;
        private LocalizationService.LocalizedTextHandle _nameHandle;
        private LocalizationService.LocalizedTextHandle _typeHandle;
        private LocalizationService.LocalizedTextHandle _descriptionHandle;

        protected override void OnLocalizationReady()
        {
            BindHeaderLocalization();
        }

        private void OnEnable()
        {
            if (Localization != null)
                BindHeaderLocalization();
            SubscribeViewModel();
        }

        private void OnDisable()
        {
            UnsubscribeViewModel();
            _lastLinkIndex = -1;
            _unknownLanguageTooltip = null;
            _viewModel?.TooltipService?.HideTooltip();

            _mainHeaderHandle?.Dispose();
            _nameHandle?.Dispose();
            _typeHandle?.Dispose();
            _descriptionHandle?.Dispose();

            _mainHeaderHandle = null;
            _nameHandle = null;
            _typeHandle = null;
            _descriptionHandle = null;

            ClearResourceIndicators();
        }

        private void Update()
        {
            if (_eventDescriptionText == null || _viewModel?.TooltipService == null)
                return;

            Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                _eventDescriptionText, mousePos, null);

            if (linkIndex == _lastLinkIndex)
                return;
            _lastLinkIndex = linkIndex;

            if (linkIndex >= 0 &&
                _eventDescriptionText.textInfo.linkInfo[linkIndex].GetLinkID() == NpcSpeechLocArg.UnknownLinkId)
            {
                _unknownLanguageTooltip ??= LocalizationService.ResolveString(
                    new LocalizedString("UI", "UI.Tooltip.UnknownLanguage"),
                    "You don't know this language", "UI.Tooltip.UnknownLanguage");
                _viewModel.TooltipService.ShowTooltipDelayed(
                    new SimpleTooltipData("", _unknownLanguageTooltip));
            }
            else
            {
                _viewModel.TooltipService.HideTooltip();
            }
        }

        private void BindHeaderLocalization()
        {
            _mainHeaderHandle?.Dispose();
            if (_mainHeader != null && _mainHeader.Text != null && _mainHeaderLocalizedString != null)
                _mainHeaderHandle = Localization.BindText(
                    _mainHeader.Text, _mainHeaderLocalizedString, "Event.MainHeader");
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as EventScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null || Localization == null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(UpdateContent);
            _locationSubscription = Observable.CombineLatest(
                    _viewModel.City, _viewModel.IsAtCity, (city, isAt) => (city, isAt))
                .Subscribe(tuple => UpdateLocation(tuple.city, tuple.isAt));
            _resultSubscription = _viewModel.SelectedChoice.Subscribe(OnSelectedChoiceChanged);
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _locationSubscription?.Dispose();
            _locationSubscription = null;
            _resultSubscription?.Dispose();
            _resultSubscription = null;
        }

        private void UpdateContent(EventData eventData)
        {
            if (eventData == null) return;

            BindLocalizedText(ref _nameHandle, _eventNameText, eventData.Name, "EventName");
            BindLocalizedText(ref _typeHandle, _eventTypeText, eventData.EventType, "EventType");

            object[] formatArgs = _viewModel.FormatArgs;
            BindLocalizedTextWithArgs(ref _descriptionHandle, _eventDescriptionText,
                eventData.Description, "EventDescription", formatArgs,
                raw => LocArgRenderer.ProcessNpcSpeech(raw, _viewModel?.LanguageRepo));

            if (eventData.Image != null)
                _eventImage.sprite = eventData.Image;

            _currentChoices = _viewModel.GetAvailableChoices();
            SpawnResourceIndicators();

            if (_currentChoices.Count == 0)
            {
                _viewModel.ConfirmResult();
                return;
            }

            CreateChoiceButtons(_currentChoices);
            _scrollViewLayout?.Refresh();
            _scrollRect?.Refresh();
        }

        private void SpawnResourceIndicators()
        {
            ClearResourceIndicators();
            if (_currentChoices == null) return;

            List<EventResourceInfo> resources = _viewModel.GetAffectedResources(_currentChoices);
            foreach (EventResourceInfo info in resources)
                SpawnResourceIndicator(info);

            if (_resourceIndicatorsRoot != null)
                _resourceIndicatorsRoot.gameObject.SetActive(resources.Count > 0);

            SetCurrentResourceValues();
        }

        private void SpawnResourceIndicator(EventResourceInfo info)
        {
            ResourceIndicator indicator = Instantiate(_resourceIndicatorPrefab, _resourceIndicatorsRoot);
            indicator.SetIcon(info.Icon);
            indicator.SetResourceType(info.ResourceType);
            indicator.HideChangeImmediate();
            _spawnedIndicators.Add(indicator);
            _resourceIndicators[info.OutcomeType] = indicator;
        }

        private void SetCurrentResourceValues()
        {
            foreach (KeyValuePair<EventOutcomeType, ResourceIndicator> kvp in _resourceIndicators)
            {
                float value = _viewModel.GetCurrentResourceValue(kvp.Value.ResourceType);
                kvp.Value.SetValue($"{value:0}");
            }
        }

        private void ClearResourceIndicators()
        {
            foreach (ResourceIndicator indicator in _spawnedIndicators)
            {
                if (indicator != null)
                    Destroy(indicator.gameObject);
            }
            _spawnedIndicators.Clear();
            _resourceIndicators.Clear();
        }

        private void BindLocalizedText(
            ref LocalizationService.LocalizedTextHandle handle,
            TextMeshProUGUI textField,
            LocalizedString localizedString,
            string fallback,
            Func<string, string> postProcess = null)
        {
            handle?.Dispose();
            if (textField == null || localizedString == null)
                return;

            if (postProcess != null)
                handle = Localization.BindText(textField, localizedString, fallback, postProcess);
            else
                handle = Localization.BindText(textField, localizedString, fallback);
        }

        private void BindLocalizedTextWithArgs(
            ref LocalizationService.LocalizedTextHandle handle,
            TextMeshProUGUI textField,
            LocalizedString localizedString,
            string fallback,
            object[] args,
            Func<string, string> postProcess = null)
        {
            handle?.Dispose();
            if (textField != null && localizedString != null)
                handle = Localization.BindText(textField, localizedString, fallback, fallback, postProcess, args);
        }

        private void CreateChoiceButtons(List<EventChoice> choices)
        {
            ClearChoiceButtons();
            if (choices == null) return;

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i;
                EventChoice choice = choices[i];
                ConditionContent condition = _viewModel.GetChoiceConditionInfo(choiceIndex, choices);
                EventChoiceButton button = Instantiate(_choiceButtonPrefab, _choiceButtonsRoot);
                button.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                button.Initialize(
                    Localization,
                    choice.Text,
                    () => _viewModel?.SelectChoice(choiceIndex),
                    null,
                    null,
                    condition);
                button.SetInteractable(true);
                _activeButtons.Add(button);
            }
        }

        private void ClearChoiceButtons()
        {
            foreach (EventChoiceButton button in _activeButtons)
                Destroy(button.gameObject);
            _activeButtons.Clear();
        }

        private void SpawnContinueButton()
        {
            EventChoiceButton continueBtn = Instantiate(_choiceButtonPrefab, _choiceButtonsRoot);
            continueBtn.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
            continueBtn.Initialize(
                Localization,
                _continueLocalizedString,
                () => _viewModel?.ConfirmResult());
            _activeButtons.Add(continueBtn);
        }

        private void OnSelectedChoiceChanged(EventChoice? choice)
        {
            if (choice == null)
                return;

            LocalizedString resultText = _viewModel.LastSkillCheckSucceeded
                ? choice.Value.ResultText
                : _viewModel.LastFailResultText;

            if (resultText != null && !resultText.IsEmpty)
            {
                _descriptionHandle?.Dispose();
                _descriptionHandle = null;
                string skillCheck = _viewModel.BuildSkillCheckLine();
                string resolved = LocalizationService.ResolveString(resultText, "Result", "Result");
                resolved = LocArgRenderer.ProcessNpcSpeech(resolved, _viewModel?.LanguageRepo);
                string summary = _viewModel.BuildOutcomeSummary();

                var parts = new List<string>();
                if (!string.IsNullOrEmpty(skillCheck)) parts.Add(skillCheck);
                parts.Add(resolved);
                if (!string.IsNullOrEmpty(summary)) parts.Add(summary);
                _eventDescriptionText.text = string.Join("\n\n", parts);
            }

            ClearChoiceButtons();
            SpawnContinueButton();

            AnimateOutcomeResults();
            _scrollViewLayout?.Refresh();
            _scrollRect?.Refresh();
        }

        private void AnimateOutcomeResults()
        {
            List<EventOutcomeEntry> outcomes = _viewModel.LastAppliedOutcomes;
            if (outcomes == null) return;

            foreach (EventOutcomeEntry outcome in outcomes)
            {
                if (!EventScreenViewModel.IsResourceOutcome(outcome.Type)) continue;

                if (!_resourceIndicators.TryGetValue(outcome.Type, out ResourceIndicator indicator))
                    continue;

                ResourceEntry entry = _viewModel.GetResourceEntry(outcome.Type);
                if (entry == null) continue;

                float newValue = _viewModel.GetCurrentResourceValue(indicator.ResourceType);
                float oldValue = newValue - outcome.Value;
                indicator.AnimateValueChange(
                    Mathf.RoundToInt(outcome.Value), entry.IncreaseIsPositive, oldValue, newValue);
            }
        }

        private void UpdateLocation(CityData city, bool isAtCity)
        {
            if (_eventLocationText == null) return;

            if (city == null)
            {
                _eventLocationText.text = "";
                return;
            }

            string cityName = LocalizationService.ResolveString(city.Name, city.Id, "CityName");

            _eventLocationText.text = isAtCity
                ? cityName
                : LocalizationService.ResolveString(_nearCityFormat, $"Near {cityName}", "NearCity", cityName);
        }
    }
}
