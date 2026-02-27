using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using R3;
using TMPro;
using UnityEngine;
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
        private readonly Dictionary<string, ResourceIndicator> _itemIndicators = new();
        private List<EventChoice> _currentChoices;
        private int _selectedChoiceIndex = -1;

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
            BindLocalizedText(ref _descriptionHandle, _eventDescriptionText, eventData.Description, "EventDescription");

            if (eventData.Image != null)
                _eventImage.sprite = eventData.Image;

            _currentChoices = _viewModel.GetAvailableChoices();
            _selectedChoiceIndex = -1;
            SpawnResourceIndicators();
            CreateChoiceButtons(_currentChoices);
        }

        private void SpawnResourceIndicators()
        {
            ClearResourceIndicators();
            if (_currentChoices == null) return;

            List<EventResourceInfo> resources = _viewModel.GetAffectedResources(_currentChoices);
            foreach (EventResourceInfo info in resources)
                SpawnResourceIndicator(info);

            HashSet<string> itemIds = _viewModel.GetAffectedItems(_currentChoices);
            foreach (string itemId in itemIds)
                SpawnItemIndicator(itemId);

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

        private void SpawnItemIndicator(string itemId)
        {
            ResourceIndicator indicator = Instantiate(_resourceIndicatorPrefab, _resourceIndicatorsRoot);
            indicator.HideChangeImmediate();
            _spawnedIndicators.Add(indicator);
            _itemIndicators[itemId] = indicator;
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
            _itemIndicators.Clear();
        }

        private void BindLocalizedText(
            ref LocalizationService.LocalizedTextHandle handle,
            TextMeshProUGUI textField,
            LocalizedString localizedString,
            string fallback)
        {
            handle?.Dispose();
            if (textField != null && localizedString != null)
                handle = Localization.BindText(textField, localizedString, fallback);
        }

        private void CreateChoiceButtons(List<EventChoice> choices)
        {
            foreach (EventChoiceButton button in _activeButtons)
                Destroy(button.gameObject);
            _activeButtons.Clear();

            if (choices == null) return;

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i;
                EventChoice choice = choices[i];
                EventChoiceButton button = Instantiate(_choiceButtonPrefab, _choiceButtonsRoot);
                button.Initialize(
                    Localization,
                    choice.Text,
                    () =>
                    {
                        _selectedChoiceIndex = choiceIndex;
                        _viewModel?.SelectChoice(choiceIndex);
                    },
                    () => ShowOutcomePreview(choiceIndex),
                    HideOutcomePreview);
                bool canAfford = _viewModel.CanAffordChoice(choiceIndex, choices);
                button.SetInteractable(canAfford);
                _activeButtons.Add(button);
            }
        }

        private void ShowOutcomePreview(int choiceIndex)
        {
            HideOutcomePreview();

            _viewModel.GetChoicePreview(choiceIndex, _currentChoices,
                out Dictionary<EventOutcomeType, float> resourceChanges,
                out Dictionary<string, float> itemChanges);

            foreach (KeyValuePair<EventOutcomeType, float> kvp in resourceChanges)
            {
                if (_resourceIndicators.TryGetValue(kvp.Key, out ResourceIndicator indicator))
                {
                    ResourceEntry entry = _viewModel.GetResourceEntry(kvp.Key);
                    if (entry != null)
                        indicator.SetChange(Mathf.RoundToInt(kvp.Value), entry.IncreaseIsPositive);

                    float current = _viewModel.GetCurrentResourceValue(indicator.ResourceType);
                    indicator.SetHighlight(kvp.Value < 0 && current + kvp.Value < 0);
                }
            }

            foreach (KeyValuePair<string, float> kvp in itemChanges)
            {
                if (_itemIndicators.TryGetValue(kvp.Key, out ResourceIndicator indicator))
                    indicator.SetChange(Mathf.RoundToInt(kvp.Value), true);
            }
        }

        private void HideOutcomePreview()
        {
            foreach (ResourceIndicator indicator in _spawnedIndicators)
            {
                indicator.HideChange();
                indicator.SetHighlight(false);
            }
        }

        private void OnSelectedChoiceChanged(EventChoice? choice)
        {
            if (choice == null)
                return;

            if (choice.Value.ResultText != null && !choice.Value.ResultText.IsEmpty)
                BindLocalizedText(ref _descriptionHandle, _eventDescriptionText, choice.Value.ResultText, "Result");

            foreach (EventChoiceButton button in _activeButtons)
                Destroy(button.gameObject);
            _activeButtons.Clear();

            EventChoiceButton continueBtn = Instantiate(_choiceButtonPrefab, _choiceButtonsRoot);
            continueBtn.Initialize(
                Localization,
                _continueLocalizedString,
                () => _viewModel?.ConfirmResult());
            _activeButtons.Add(continueBtn);

            SetCurrentResourceValues();
            if (_selectedChoiceIndex >= 0)
                ShowOutcomePreview(_selectedChoiceIndex);
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
