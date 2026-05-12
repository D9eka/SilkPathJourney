using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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
        [SerializeField] private TextMeshProUGUI _eventDescriptionTextNoImage;
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

        private ComponentPool<EventChoiceButton> _buttonPool;
        private EventScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _locationSubscription;
        private IDisposable _resultSubscription;
        private readonly List<EventChoiceButton> _activeButtons = new();
        private readonly List<ResourceIndicator> _spawnedIndicators = new();
        private readonly Dictionary<EventOutcomeType, ResourceIndicator> _resourceIndicators = new();
        private List<EventChoice> _currentChoices;
        private bool _choiceMade;
        private int _lastLinkIndex = -1;
        private string _unknownLanguageTooltip;
        private TextMeshProUGUI _activeDescriptionText;

        private LocalizationService.LocalizedTextHandle _mainHeaderHandle;
        private LocalizationService.LocalizedTextHandle _nameHandle;
        private LocalizationService.LocalizedTextHandle _typeHandle;
        private LocalizationService.LocalizedTextHandle _descriptionHandle;

        protected override void Awake()
        {
            base.Awake();
            _buttonPool = new ComponentPool<EventChoiceButton>(_choiceButtonPrefab, _choiceButtonsRoot);
        }

        protected override void OnLocalizationReady()
        {
            BindHeaderLocalization();
        }

        private void OnEnable()
        {
            if (Localization != null)
                BindHeaderLocalization();
            SubscribeViewModel();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
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
            TextMeshProUGUI textForLink = _activeDescriptionText ?? _eventDescriptionText;
            if (textForLink == null || _viewModel?.TooltipService == null)
                return;

            Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                textForLink, mousePos, null);

            if (linkIndex == _lastLinkIndex)
                return;
            _lastLinkIndex = linkIndex;

            if (linkIndex >= 0 &&
                textForLink.textInfo.linkInfo[linkIndex].GetLinkID() == NpcSpeechLocArg.UnknownLinkId)
            {
                _unknownLanguageTooltip ??= LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_UnknownLanguage);
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

        private void OnLocaleChanged(Locale _)
        {
            if (_choiceMade || _currentChoices == null || _currentChoices.Count == 0)
                return;
            CreateChoiceButtons(_currentChoices);
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as EventScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || Localization == null
                || _stateSubscription != null || _locationSubscription != null || _resultSubscription != null)
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

        private void RequestLayoutRefresh()
        {
            if (_scrollViewLayout != null && _scrollViewLayout.gameObject.activeInHierarchy)
                _scrollViewLayout.Refresh();
            if (_scrollRect != null && _scrollRect.gameObject.activeInHierarchy)
                _scrollRect.Refresh();
        }

        private void UpdateContent(EventData eventData)
        {
            if (eventData == null) return;

            ClearChoiceButtons();

            try
            {
                BindLocalizedText(ref _nameHandle, _eventNameText, eventData.Name, "EventName");
                BindLocalizedText(ref _typeHandle, _eventTypeText, eventData.EventType, "EventType");

                bool hasImage = eventData.Image != null;
                if (_eventImage != null)
                {
                    _eventImage.gameObject.SetActive(hasImage);
                    if (hasImage) _eventImage.sprite = eventData.Image;
                }
                if (_scrollRect != null)
                    _scrollRect.gameObject.SetActive(hasImage);
                if (_eventDescriptionTextNoImage != null)
                    _eventDescriptionTextNoImage.gameObject.SetActive(!hasImage);

                _activeDescriptionText = hasImage ? _eventDescriptionText : _eventDescriptionTextNoImage;

                object[] formatArgs = _viewModel.FormatArgs;
                BindLocalizedTextWithArgs(ref _descriptionHandle, _activeDescriptionText,
                    eventData.Description, "EventDescription", formatArgs,
                    raw => LocArgRenderer.ProcessNpcSpeech(raw, _viewModel?.LanguageRepo));

                if (_nameHandle != null) _nameHandle.TextChanged += RequestLayoutRefresh;
                if (_typeHandle != null) _typeHandle.TextChanged += RequestLayoutRefresh;
                if (_descriptionHandle != null) _descriptionHandle.TextChanged += RequestLayoutRefresh;

                _currentChoices = eventData.Choices != null
                    ? new List<EventChoice>(eventData.Choices)
                    : new List<EventChoice>();
                _choiceMade = false;
                SpawnResourceIndicators();

                if (_currentChoices.Count == 0)
                {
                    Debug.LogError($"[SPJ Events] Event '{eventData.Id}' has no choices. Data issue?");
                    return;
                }

                CreateChoiceButtons(_currentChoices);
                RequestLayoutRefresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SPJ Events] UpdateContent threw for '{eventData.Id}': {ex}");
            }
        }

        private void SpawnResourceIndicators()
        {
            ClearResourceIndicators();
            if (_currentChoices == null) return;

            List<EventResourceInfo> resources = _viewModel.GetAffectedResources(_currentChoices);

            if (_resourceIndicatorsRoot != null)
                _resourceIndicatorsRoot.gameObject.SetActive(resources.Count > 0);

            foreach (EventResourceInfo info in resources)
                SpawnResourceIndicator(info);

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
                EventChoiceButton button = _buttonPool.Get();
                button.gameObject.InitializeColorBinders(_viewModel?.ThemeService);
                button.Initialize(
                    Localization,
                    choice.Text,
                    () => _viewModel?.SelectChoice(choiceIndex),
                    null,
                    null,
                    condition);
                button.SetInteractable(_viewModel.CanSelectChoice(choiceIndex, choices));
                _activeButtons.Add(button);
            }
        }

        private void ClearChoiceButtons()
        {
            foreach (EventChoiceButton btn in _activeButtons)
                _buttonPool.Release(btn);
            _activeButtons.Clear();
        }

        private void SpawnContinueButton()
        {
            EventChoiceButton continueBtn = _buttonPool.Get();
            continueBtn.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
            continueBtn.Initialize(
                Localization,
                _continueLocalizedString,
                () => _viewModel?.ConfirmResult());
            continueBtn.SetInteractable(true);
            _activeButtons.Add(continueBtn);
        }

        private void OnSelectedChoiceChanged(EventChoice? choice)
        {
            if (choice == null)
                return;

            if (_currentChoices == null || _currentChoices.Count == 0)
                return;
            bool belongsToCurrentEvent = false;
            foreach (var c in _currentChoices)
            {
                if (ReferenceEquals(c.Text, choice.Value.Text))
                {
                    belongsToCurrentEvent = true;
                    break;
                }
            }
            if (!belongsToCurrentEvent)
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
                TextMeshProUGUI target = _activeDescriptionText ?? _eventDescriptionText;
                if (target != null)
                    target.text = string.Join("\n\n", parts);
            }

            _choiceMade = true;
            ClearChoiceButtons();
            SpawnContinueButton();

            AnimateOutcomeResults();
            (_activeDescriptionText ?? _eventDescriptionText)?.ForceMeshUpdate();
            RequestLayoutRefresh();
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
            RequestLayoutRefresh();
        }
    }
}
