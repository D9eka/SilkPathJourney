using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.ViewModel;
using Internal.Scripts.UI.StackService;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Event
{
    public sealed class EventScreenViewModel : ScreenViewModelBase
    {
        private readonly EventTrigger _eventTrigger;
        private readonly ScreenStackService _screenStackService;
        private readonly ItemCatalog _itemCatalog;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly ReactiveProperty<EventData> _state = new(null);
        private readonly ReactiveProperty<CityData> _city = new(null);
        private readonly ReactiveProperty<bool> _isAtCity = new(false);
        private readonly ReactiveProperty<EventChoice?> _selectedChoice = new(null);

        public EventScreenViewModel(
            EventTrigger eventTrigger,
            ScreenStackService screenStackService,
            ItemCatalog itemCatalog,
            ResourceIconCatalog resourceIcons,
            PlayerResourceRepository resourceRepository)
        {
            _eventTrigger = eventTrigger;
            _screenStackService = screenStackService;
            _itemCatalog = itemCatalog;
            _resourceIcons = resourceIcons;
            _resourceRepository = resourceRepository;
        }

        public override ScreenId Id => ScreenId.Event;

        public Observable<EventData> State => _state;
        public Observable<CityData> City => _city;
        public Observable<bool> IsAtCity => _isAtCity;
        public Observable<EventChoice?> SelectedChoice => _selectedChoice;
        public ItemCatalog ItemCatalog => _itemCatalog;
        public EventTrigger EventTrigger => _eventTrigger;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public PlayerResourceState PlayerResources => _resourceRepository.Current;

        protected override void OnOpen(object args)
        {
            if (args is EventTriggerArgs triggerArgs)
            {
                _state.Value = triggerArgs.EventData;
                _city.Value = triggerArgs.City;
                _isAtCity.Value = triggerArgs.IsAtCity;
            }
            else
            {
                _state.Value = args as EventData;
                _city.Value = null;
                _isAtCity.Value = false;
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
            _eventTrigger.OnEventCompleted();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (_state.Value == null || choiceIndex < 0 ||
                choiceIndex >= _state.Value.Choices.Count)
                return;

            EventChoice choice = _state.Value.Choices[choiceIndex];
            _eventTrigger.ApplyOutcome(choice.Outcomes);
            _selectedChoice.Value = choice;
        }

        public void ConfirmResult()
        {
            _selectedChoice.Value = null;
            _screenStackService.Close(ScreenId.Event);
        }
    }
}
