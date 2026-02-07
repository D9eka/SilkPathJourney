using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
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

        private EventScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _locationSubscription;
        private readonly List<EventChoiceButton> _activeButtons = new();
        private readonly List<ResourceIndicator> _spawnedIndicators = new();
        private readonly Dictionary<EventOutcomeType, ResourceIndicator> _resourceIndicators = new();
        private readonly Dictionary<string, ResourceIndicator> _itemIndicators = new();

        private LocalizationHelper.LocalizedTextHandle _nameHandle;
        private LocalizationHelper.LocalizedTextHandle _typeHandle;
        private LocalizationHelper.LocalizedTextHandle _descriptionHandle;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as EventScreenViewModel;
            EnsureGraphicRaycaster();
            SubscribeViewModel();
        }

        private void OnEnable()
        {
            SubscribeViewModel();
        }

        private void EnsureGraphicRaycaster()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        private static readonly LocalizedString NearCityFormat = new("UI", "ui.event.near_city");

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(UpdateContent);
            _locationSubscription = Observable.CombineLatest(
                    _viewModel.City, _viewModel.IsAtCity, (city, isAt) => (city, isAt))
                .Subscribe(tuple => UpdateLocation(tuple.city, tuple.isAt));
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _locationSubscription?.Dispose();
            _locationSubscription = null;
        }

        private void UpdateContent(EventData eventData)
        {
            if (eventData == null)
                return;

            BindLocalizedText(ref _nameHandle, _eventNameText, eventData.Name, "EventName");
            BindLocalizedText(ref _typeHandle, _eventTypeText, eventData.EventType, "EventType");
            BindLocalizedText(ref _descriptionHandle, _eventDescriptionText, eventData.Description, "EventDescription");

            if (_eventImage != null)
            {
                bool hasImage = eventData.Image != null;
                _eventImage.gameObject.SetActive(hasImage);
                if (hasImage)
                    _eventImage.sprite = eventData.Image;
            }

            var availableChoices = GetAvailableChoices(eventData.Choices);
            SpawnResourceIndicators(availableChoices);
            CreateChoiceButtons(availableChoices);
        }

        private List<EventChoice> GetAvailableChoices(List<EventChoice> choices)
        {
            if (choices == null)
                return new List<EventChoice>();

            var eventTrigger = _viewModel?.EventTrigger;
            if (eventTrigger == null)
                return new List<EventChoice>(choices);

            return choices.Where(c =>
                c.Conditions == null || c.Conditions.Count == 0 ||
                eventTrigger.CheckConditions(c.Conditions)).ToList();
        }

        private void SpawnResourceIndicators(List<EventChoice> choices)
        {
            ClearResourceIndicators();

            if (choices == null || _resourceIndicatorPrefab == null || _resourceIndicatorsRoot == null)
                return;

            HashSet<string> itemIds = new();
            bool hasMoney = false;
            bool hasFood = false;
            bool hasDanger = false;
            bool hasCart = false;

            foreach (var choice in choices)
            {
                if (choice.Outcomes == null) continue;
                foreach (var outcome in choice.Outcomes)
                {
                    switch (outcome.Type)
                    {
                        case EventOutcomeType.Money: hasMoney = true; break;
                        case EventOutcomeType.Food: hasFood = true; break;
                        case EventOutcomeType.Danger: hasDanger = true; break;
                        case EventOutcomeType.CartDurability: hasCart = true; break;
                        case EventOutcomeType.AddItem:
                            if (!string.IsNullOrEmpty(outcome.Param))
                                itemIds.Add(outcome.Param);
                            break;
                    }
                }
            }

            var icons = _viewModel?.ResourceIcons;
            if (hasMoney) SpawnResourceIndicator(EventOutcomeType.Money, icons?.Money);
            if (hasFood) SpawnResourceIndicator(EventOutcomeType.Food, icons?.Food);
            if (hasDanger) SpawnResourceIndicator(EventOutcomeType.Danger, icons?.Danger);
            if (hasCart) SpawnResourceIndicator(EventOutcomeType.CartDurability, icons?.PlayerCartDurability);

            foreach (var itemId in itemIds)
                SpawnItemIndicator(itemId, null);

            SetCurrentResourceValues();
        }

        private void SpawnResourceIndicator(EventOutcomeType type, Sprite icon)
        {
            var indicator = Instantiate(_resourceIndicatorPrefab, _resourceIndicatorsRoot);
            indicator.SetIcon(icon);
            indicator.HideChangeImmediate();
            _spawnedIndicators.Add(indicator);
            _resourceIndicators[type] = indicator;
        }

        private void SpawnItemIndicator(string itemId, Sprite icon)
        {
            var indicator = Instantiate(_resourceIndicatorPrefab, _resourceIndicatorsRoot);
            indicator.SetIcon(icon);
            indicator.HideChangeImmediate();
            _spawnedIndicators.Add(indicator);
            _itemIndicators[itemId] = indicator;
        }

        private void SetCurrentResourceValues()
        {
            var resources = _viewModel?.PlayerResources;
            if (resources == null) return;

            if (_resourceIndicators.TryGetValue(EventOutcomeType.Money, out var money))
                money.SetValue(resources.Money);
            if (_resourceIndicators.TryGetValue(EventOutcomeType.Food, out var food))
                food.SetValue($"{resources.Food:0}");
            if (_resourceIndicators.TryGetValue(EventOutcomeType.Danger, out var danger))
                danger.SetValue($"{resources.AccumulatedDanger:0}");
            if (_resourceIndicators.TryGetValue(EventOutcomeType.CartDurability, out var cart))
            {
                float avg = resources.Carts != null && resources.Carts.Count > 0
                    ? resources.Carts.Average(c => c.Durability) : 0f;
                cart.SetValue($"{avg:0}");
            }
        }

        private void ClearResourceIndicators()
        {
            foreach (var indicator in _spawnedIndicators)
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
            foreach (var button in _activeButtons)
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
            foreach (var indicator in _spawnedIndicators)
                indicator.HideChange();

            if (outcomes == null) return;

            Dictionary<EventOutcomeType, float> resourceChanges = new();
            Dictionary<string, float> itemChanges = new();

            foreach (var entry in outcomes)
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

            foreach (var kvp in resourceChanges)
            {
                if (_resourceIndicators.TryGetValue(kvp.Key, out var indicator))
                    indicator.SetChange(Mathf.RoundToInt(kvp.Value));
            }

            foreach (var kvp in itemChanges)
            {
                if (_itemIndicators.TryGetValue(kvp.Key, out var indicator))
                    indicator.SetChange(Mathf.RoundToInt(kvp.Value));
            }
        }

        private void HideOutcomePreview()
        {
            foreach (var indicator in _spawnedIndicators)
                indicator.HideChange();
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
                : LocalizationHelper.ResolveString(NearCityFormat, $"Near {cityName}", "NearCity", cityName);
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
    }
}
