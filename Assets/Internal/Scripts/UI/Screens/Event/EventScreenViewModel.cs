using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Screens.Event.ConditionLines;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using R3;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Event
{
    public class ConditionContent
    {
        public readonly string Format;
        public readonly IReadOnlyList<ILocArg> Args;

        public ConditionContent(string format, IReadOnlyList<ILocArg> args)
        {
            Format = format;
            Args = args;
        }
    }

    public readonly struct EventResourceInfo
    {
        public readonly EventOutcomeType OutcomeType;
        public readonly ResourceType ResourceType;
        public readonly Sprite Icon;
        public readonly bool IncreaseIsPositive;

        public EventResourceInfo(EventOutcomeType outcomeType, ResourceType resourceType, Sprite icon, bool increaseIsPositive)
        {
            OutcomeType = outcomeType;
            ResourceType = resourceType;
            Icon = icon;
            IncreaseIsPositive = increaseIsPositive;
        }
    }

    public sealed class EventScreenViewModel : ScreenViewModelBase
    {
        private readonly EventTrigger _eventTrigger;
        private readonly ScreenStackService _screenStackService;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ConditionLineBuilder _conditionLineBuilder;
        private readonly SkillCheckService _skillCheckService;
        private readonly EventSelector _eventSelector;
        private readonly EventOutcomeFormatter _formatter;
        private readonly PlayerLanguageRepository _languageRepo;
        private readonly TooltipService _tooltipService;
        private List<EventOutcomeEntry> _lastAppliedOutcomes;
        private readonly ReactiveProperty<EventData> _state = new(null);
        private readonly ReactiveProperty<CityData> _city = new(null);
        private readonly ReactiveProperty<bool> _isAtCity = new(false);
        private readonly ReactiveProperty<EventChoice?> _selectedChoice = new(null);
        private object[] _formatArgs;
        private SkillCheckData? _lastSkillCheck;

        public bool LastSkillCheckSucceeded { get; private set; } = true;
        public LocalizedString LastFailResultText => _lastSkillCheck?.FailResultText;

        public EventScreenViewModel(
            EventTrigger eventTrigger,
            ScreenStackService screenStackService,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons,
            PlayerResourceRepository resourceRepository,
            InventoryRepository inventoryRepository,
            ConditionLineBuilder conditionLineBuilder,
            SkillCheckService skillCheckService,
            EventSelector eventSelector,
            EventOutcomeFormatter formatter,
            PlayerLanguageRepository languageRepo,
            TooltipService tooltipService)
        {
            _eventTrigger = eventTrigger;
            _screenStackService = screenStackService;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
            _resourceRepository = resourceRepository;
            _inventoryRepository = inventoryRepository;
            _conditionLineBuilder = conditionLineBuilder;
            _skillCheckService = skillCheckService;
            _eventSelector = eventSelector;
            _formatter = formatter;
            _languageRepo = languageRepo;
            _tooltipService = tooltipService;
        }

        public override ScreenId Id => ScreenId.Event;

        public Observable<EventData> State => _state;
        public Observable<CityData> City => _city;
        public Observable<bool> IsAtCity => _isAtCity;
        public Observable<EventChoice?> SelectedChoice => _selectedChoice;
        public object[] FormatArgs => _formatArgs;
        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public PlayerLanguageRepository LanguageRepo => _languageRepo;
        public TooltipService TooltipService => _tooltipService;
        public PlayerResourceState PlayerResources => _resourceRepository.Current;

        protected override void OnOpen(object args)
        {
            if (args is EventTriggerArgs triggerArgs)
            {
                _formatArgs = triggerArgs.FormatArgs;
                _state.Value = triggerArgs.EventData;
                _city.Value = triggerArgs.City;
                _isAtCity.Value = triggerArgs.IsAtCity;
            }
            else
            {
                _state.Value = args as EventData;
                _city.Value = null;
                _isAtCity.Value = false;
                _formatArgs = null;
            }

            if (_state.Value == null)
            {
                Debug.LogError("[SPJ Events] EventScreenViewModel opened without EventData");
                _screenStackService.Close(ScreenId.Event);
            }
        }

        protected override void OnClose()
        {
            _state.Value = null;
            _city.Value = null;
            _isAtCity.Value = false;
            _selectedChoice.Value = null;
            _formatArgs = null;
            _eventTrigger.OnEventCompleted();
        }

        public List<EventChoice> GetAvailableChoices()
        {
            EventData eventData = _state.Value;
            if (eventData?.Choices == null)
                return new List<EventChoice>();

            return eventData.Choices.Where(c =>
                c.Conditions == null || c.Conditions.Count == 0 ||
                _eventSelector.CheckConditions(c.Conditions)).ToList();
        }

        public List<EventResourceInfo> GetAffectedResources(List<EventChoice> choices)
        {
            HashSet<EventOutcomeType> outcomeTypes = new();
            if (choices != null)
            {
                foreach (EventChoice choice in choices)
                {
                    if (choice.Outcomes == null) continue;
                    foreach (EventOutcomeEntry outcome in choice.Outcomes)
                    {
                        if (outcome.Type != EventOutcomeType.None && outcome.Type != EventOutcomeType.AddItem)
                            outcomeTypes.Add(outcome.Type);
                    }
                }
            }

            List<EventResourceInfo> result = new();
            foreach (EventOutcomeType type in outcomeTypes)
            {
                ResourceEntry entry = GetResourceEntry(type);
                if (entry != null)
                    result.Add(new EventResourceInfo(type, entry.Type, entry.Icon, entry.IncreaseIsPositive));
            }
            return result;
        }

        public HashSet<string> GetAffectedItems(List<EventChoice> choices)
        {
            HashSet<string> itemIds = new();
            if (choices != null)
            {
                foreach (EventChoice choice in choices)
                {
                    if (choice.Outcomes == null) continue;
                    foreach (EventOutcomeEntry outcome in choice.Outcomes)
                    {
                        if (outcome.Type == EventOutcomeType.AddItem && !string.IsNullOrEmpty(outcome.Param))
                            itemIds.Add(outcome.Param);
                    }
                }
            }
            return itemIds;
        }

        public void GetChoicePreview(int choiceIndex, List<EventChoice> choices,
            out Dictionary<EventOutcomeType, float> resourceChanges,
            out Dictionary<string, float> itemChanges)
        {
            resourceChanges = new Dictionary<EventOutcomeType, float>();
            itemChanges = new Dictionary<string, float>();

            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Count)
                return;

            List<EventOutcomeEntry> outcomes = choices[choiceIndex].Outcomes;
            if (outcomes == null) return;

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
        }

        public ResourceEntry GetResourceEntry(EventOutcomeType type)
        {
            ResourceType? resType = _eventTrigger?.GetAffectedResource(type);
            return resType.HasValue ? _resourceIcons?.Get(resType.Value) : null;
        }

        public float GetCurrentResourceValue(ResourceType type)
        {
            if (type == ResourceType.Food)
                return InventoryStateMutator.GetItemCount(
                    _inventoryRepository.GetPlayerInventory(), SuppliesItemId.Value);

            return _resourceRepository.Current?.GetValue(type) ?? 0f;
        }

        public bool CanAffordChoice(int choiceIndex, List<EventChoice> choices)
        {
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Count)
                return true;
            return _eventTrigger.CanAffordOutcomes(choices[choiceIndex].Outcomes);
        }

        public void SelectChoice(int choiceIndex)
        {
            List<EventChoice> available = GetAvailableChoices();
            if (choiceIndex < 0 || choiceIndex >= available.Count)
                return;

            if (!CanAffordChoice(choiceIndex, available))
                return;

            EventChoice choice = available[choiceIndex];
            int originalIndex = GetOriginalChoiceIndex(choice);
            _eventTrigger.LastChoiceIndex = choiceIndex;

            SkillCheckData? skillCheck = _state.Value?.GetSkillCheck(originalIndex);
            if (skillCheck.HasValue)
            {
                LastSkillCheckSucceeded = _skillCheckService.RollSkillCheck(
                    skillCheck.Value.SkillType, skillCheck.Value.BaseChance);
                _lastSkillCheck = skillCheck.Value;
                _lastAppliedOutcomes = LastSkillCheckSucceeded ? choice.Outcomes : skillCheck.Value.FailOutcomes;
                _eventTrigger.ApplyOutcome(_lastAppliedOutcomes);
            }
            else
            {
                LastSkillCheckSucceeded = true;
                _lastSkillCheck = null;
                _lastAppliedOutcomes = choice.Outcomes;
                _eventTrigger.ApplyOutcome(_lastAppliedOutcomes);
            }

            _selectedChoice.Value = choice;
        }

        public ConditionContent GetChoiceConditionInfo(int choiceIndex, List<EventChoice> choices)
        {
            if (_state.Value == null || choices == null || choiceIndex < 0 || choiceIndex >= choices.Count)
                return null;

            EventChoice choice = choices[choiceIndex];
            int originalIndex = GetOriginalChoiceIndex(choice);

            var ctx = new ConditionLineContext(_state.Value, choice, originalIndex);
            string text = _conditionLineBuilder.Build(ctx);

            return text == null ? null : new ConditionContent(text, null);
        }

        private int GetOriginalChoiceIndex(EventChoice choice)
        {
            EventData eventData = _state.Value;
            if (eventData?.Choices == null) return -1;

            for (int i = 0; i < eventData.Choices.Count; i++)
            {
                if (ReferenceEquals(eventData.Choices[i].Text, choice.Text) &&
                    ReferenceEquals(eventData.Choices[i].ResultText, choice.ResultText))
                    return i + 1;
            }

            return -1;
        }

        public string BuildSkillCheckLine()
        {
            if (!_lastSkillCheck.HasValue) return null;
            return _formatter.BuildSkillCheckLine(_lastSkillCheck.Value, LastSkillCheckSucceeded);
        }

        public string BuildOutcomeSummary()
        {
            return _formatter.BuildOutcomeSummary(_lastAppliedOutcomes);
        }

        public void ConfirmResult()
        {
            _selectedChoice.Value = null;
            _screenStackService.Close(ScreenId.Event);
        }
    }
}
