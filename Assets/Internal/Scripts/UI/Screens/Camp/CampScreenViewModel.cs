using System;
using System.Collections.Generic;
using Internal.Scripts.Camp;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.World.State;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Camp
{
    public sealed class CampScreenViewModel : ScreenViewModelBase
    {
        private readonly CampActionService _campActionService;
        private readonly CampController _campController;
        private readonly ResourceIconCatalog _iconCatalog;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly InventoryRepository _inventoryRepository;
        private readonly DayTracker _dayTracker;
        private readonly GameClock _gameClock;
        private readonly CampActionButtonFactory _buttonFactory;

        private readonly ReactiveProperty<CampViewState> _state = new();
        private IDisposable _resourceSubscription;
        private IDisposable _inventorySubscription;

        public Observable<CampViewState> State => _state;
        public override ScreenId Id => ScreenId.Camp;
        public ResourceIconCatalog ResourceIcons => _iconCatalog;
        public int CurrentDay => _dayTracker.CurrentDay;
        public event Action<int> DayChanged;

        private static readonly CampActionType[] ActionOrder =
        {
            CampActionType.Forage,
            CampActionType.Rest,
            CampActionType.Repair,
            CampActionType.Order
        };

        private static readonly ResourceType[] ResourceOrder =
        {
            ResourceType.Food,
            ResourceType.PlayerCartDurability,
            ResourceType.Morale,
            ResourceType.OtherCartsDurability,
            ResourceType.Danger
        };

        public CampScreenViewModel(
            CampActionService campActionService,
            CampController campController,
            ResourceIconCatalog iconCatalog,
            PlayerResourceRepository resourceRepo,
            InventoryRepository inventoryRepository,
            DayTracker dayTracker,
            GameClock gameClock,
            CampActionButtonFactory buttonFactory)
        {
            _campActionService = campActionService;
            _campController = campController;
            _iconCatalog = iconCatalog;
            _resourceRepo = resourceRepo;
            _inventoryRepository = inventoryRepository;
            _dayTracker = dayTracker;
            _gameClock = gameClock;
            _buttonFactory = buttonFactory;
        }

        protected override void OnOpen(object args)
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            _dayTracker.OnDayChanged += HandleDayChanged;
            _gameClock.Pause();
            BuildState();
            _resourceSubscription = _resourceRepo.StateStream.Subscribe(_ => BuildState());
            _inventorySubscription = _inventoryRepository.PlayerInventoryStream.Subscribe(_ => BuildState());
        }

        protected override void OnClose()
        {
            _resourceSubscription?.Dispose();
            _resourceSubscription = null;
            _inventorySubscription?.Dispose();
            _inventorySubscription = null;
            _gameClock.Resume();
            _dayTracker.OnDayChanged -= HandleDayChanged;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _state.Value = null;
        }

        private void HandleDayChanged(int day) => DayChanged?.Invoke(day);

        public void ExecuteAction(CampActionType type)
        {
            _campController.ExecuteActionAndAdvance(type);
        }

        private void OnLocaleChanged(Locale _) => BuildState();

        private void BuildState()
        {
            var current = _resourceRepo.Current;

            var resourceValues = new Dictionary<ResourceType, float>(ResourceOrder.Length);
            foreach (var type in ResourceOrder)
            {
                resourceValues[type] = type == ResourceType.Food
                    ? InventoryStateMutator.GetItemCount(
                        _inventoryRepository.GetPlayerInventory(), SuppliesItemId.Value)
                    : current.GetValue(type);
            }

            var buttons = new CampActionButtonState[ActionOrder.Length];
            for (int i = 0; i < ActionOrder.Length; i++)
                buttons[i] = _buttonFactory.Build(ActionOrder[i]);

            _state.Value = new CampViewState(resourceValues, buttons);
        }
    }
}
