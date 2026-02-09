using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screen.View;
using Internal.Scripts.UI.Screen.ViewModel;
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

        private LocalizationHelper.LocalizedTextHandle _nameHandle;
        private LocalizationHelper.LocalizedTextHandle _typeHandle;
        private LocalizationHelper.LocalizedTextHandle _descriptionHandle;

        private void OnEnable()
        {
            SubscribeViewModel();
        }

        private void OnDisable()
        {
            UnsubscribeViewModel();

            _nameHandle?.Dispose();
            _typeHandle?.Dispose();
            _descriptionHandle?.Dispose();

            _nameHandle = null;
            _typeHandle = null;
            _descriptionHandle = null;

            ClearResourceIndicators();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as EventScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
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

            List<EventChoice> availableChoices = GetAvailableChoices(eventData.Choices);
            SpawnResourceIndicators(availableChoices);
            CreateChoiceButtons(availableChoices);
        }

        private List<EventChoice> GetAvailableChoices(List<EventChoice> choices)
        {
            if (choices == null)
                return new List<EventChoice>();

            EventTrigger eventTrigger = _viewModel.EventTrigger;
            if (eventTrigger == null)
                return new List<EventChoice>(choices);

            return choices.Where(c =>
                c.Conditions == null || c.Conditions.Count == 0 ||
                eventTrigger.CheckConditions(c.Conditions)).ToList();
        }

        private void SpawnResourceIndicators(List<EventChoice> choices)
        {
            ClearResourceIndicators();

            if (choices == null) return;

            HashSet<EventOutcomeType> outcomeTypes = new();
            HashSet<string> itemIds = new();

            foreach (EventChoice choice in choices)
            {
                if (choice.Outcomes == null) continue;
                foreach (EventOutcomeEntry outcome in choice.Outcomes)
                {
                    if (outcome.Type == EventOutcomeType.AddItem)
                    {
                        if (!string.IsNullOrEmpty(outcome.Param))
                            itemIds.Add(outcome.Param);
                    }
                    else if (outcome.Type != EventOutcomeType.None)
                    {
                        outcomeTypes.Add(outcome.Type);
                    }
                }
            }

            ResourceIconCatalog catalog = _viewModel.ResourceIcons;
            EventTrigger trigger = _viewModel.EventTrigger;

            foreach (EventOutcomeType type in outcomeTypes)
            {
                ResourceType? resType = trigger?.GetAffectedResource(type);
                ResourceEntry entry = resType.HasValue ? catalog?.Get(resType.Value) : null;
                if (entry != null)
                {
                    SpawnResourceIndicator(type, entry);
                }
            }

            foreach (string itemId in itemIds)
                SpawnItemIndicator(itemId, null);

            SetCurrentResourceValues();
        }

        private void SpawnResourceIndicator(EventOutcomeType type, ResourceEntry entry)
        {
            ResourceIndicator indicator = Instantiate(_resourceIndicatorPrefab, _resourceIndicatorsRoot);
            indicator.SetIcon(entry?.Icon);
            if (entry != null) indicator.SetResourceType(entry.Type);
            indicator.HideChangeImmediate();
            _spawnedIndicators.Add(indicator);
            _resourceIndicators[type] = indicator;
        }

        private void SpawnItemIndicator(string itemId, Sprite icon)
        {
            ResourceIndicator indicator = Instantiate(_resourceIndicatorPrefab, _resourceIndicatorsRoot);
            indicator.SetIcon(icon);
            indicator.HideChangeImmediate();
            _spawnedIndicators.Add(indicator);
            _itemIndicators[itemId] = indicator;
        }

        private void SetCurrentResourceValues()
        {
            PlayerResourceState resources = _viewModel.PlayerResources;
            if (resources == null) return;

            foreach (KeyValuePair<EventOutcomeType, ResourceIndicator> kvp in _resourceIndicators)
            {
                float value = resources.GetValue(kvp.Value.ResourceType);
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
            ref LocalizationHelper.LocalizedTextHandle handle,
            TextMeshProUGUI textField,
            LocalizedString localizedString,
            string fallback)
        {
            handle?.Dispose();
            if (textField != null && localizedString != null)
                handle = LocalizationHelper.BindText(textField, localizedString, fallback);
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
                    choice.Text,
                    () => _viewModel?.SelectChoice(choiceIndex),
                    () => ShowOutcomePreview(choice.Outcomes),
                    HideOutcomePreview);
                _activeButtons.Add(button);
            }
        }

        private void ShowOutcomePreview(List<EventOutcomeEntry> outcomes)
        {
            foreach (ResourceIndicator indicator in _spawnedIndicators)
                indicator.HideChange();

            if (outcomes == null) return;

            Dictionary<EventOutcomeType, float> resourceChanges = new();
            Dictionary<string, float> itemChanges = new();

            foreach (EventOutcomeEntry entry in outcomes)
            {
                if (entry.Type == EventOutcomeType.AddItem)
                {
                    if (!string.IsNullOrEmpty(entry.Param))
                    {
                        if (itemChanges.ContainsKey(entry.Param))
                            itemChanges[entry.Param] += entry.Value;
                        else
                            itemChanges[entry.Param] = entry.Value;
                    }
                }
                else if (entry.Type != EventOutcomeType.None)
                {
                    if (resourceChanges.ContainsKey(entry.Type))
                        resourceChanges[entry.Type] += entry.Value;
                    else
                        resourceChanges[entry.Type] = entry.Value;
                }
            }

            foreach (KeyValuePair<EventOutcomeType, float> kvp in resourceChanges)
            {
                if (_resourceIndicators.TryGetValue(kvp.Key, out ResourceIndicator indicator))
                {
                    ResourceType? resType = _viewModel?.EventTrigger?.GetAffectedResource(kvp.Key);
                    ResourceEntry entry = resType.HasValue ? _viewModel?.ResourceIcons?.Get(resType.Value) : null;
                    if (entry != null)
                    {
                        indicator.SetChange(Mathf.RoundToInt(kvp.Value), entry.IncreaseIsPositive);
                    }
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
                indicator.HideChange();
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
                _continueLocalizedString,
                () => _viewModel?.ConfirmResult());
            _activeButtons.Add(continueBtn);

            SetCurrentResourceValues();
            ShowOutcomePreview(choice.Value.Outcomes);
        }

        private void UpdateLocation(CityData city, bool isAtCity)
        {
            if (_eventLocationText == null) return;

            if (city == null)
            {
                _eventLocationText.text = "";
                return;
            }

            string cityName = LocalizationHelper.ResolveString(city.Name, city.Id, "CityName");

            _eventLocationText.text = isAtCity
                ? cityName
                : LocalizationHelper.ResolveString(_nearCityFormat, $"Near {cityName}", "NearCity", cityName);
        }
    }
}
