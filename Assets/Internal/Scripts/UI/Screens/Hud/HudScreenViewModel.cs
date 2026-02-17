using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Hud
{
    public sealed class HudScreenViewModel : ScreenViewModelBase
    {
        private readonly HudModel _model;
        private readonly ScreenStackService _screenStackService;
        private readonly ICityEntryService _cityEntryService;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerMovementControl _playerMovementControl;
        private readonly IPlayerTurnChoiceState _turnChoiceState;
        private readonly IArrowsController _arrowsController;
        private readonly EventToastController _toastController;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly DayTracker _dayTracker;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly ResourceIconCatalog _iconCatalog;

        public event Action<bool> InteractableChanged;
        public event Action<bool> VisibilityChanged;
        public event Action<int> DayChanged;

        public HudScreenViewModel(
            HudModel model,
            ScreenStackService screenStackService,
            ICityEntryService cityEntryService,
            IPlayerStateProvider playerStateProvider,
            IPlayerMovementControl playerMovementControl,
            IPlayerTurnChoiceState turnChoiceState,
            IArrowsController arrowsController,
            EventToastController toastController,
            PlayerResourceRepository resourceRepository,
            DayTracker dayTracker,
            GameBalanceConfig balanceConfig,
            ResourceIconCatalog iconCatalog)
        {
            _model = model;
            _screenStackService = screenStackService;
            _cityEntryService = cityEntryService;
            _playerStateProvider = playerStateProvider;
            _playerMovementControl = playerMovementControl;
            _turnChoiceState = turnChoiceState;
            _arrowsController = arrowsController;
            _toastController = toastController;
            _resourceRepository = resourceRepository;
            _dayTracker = dayTracker;
            _balanceConfig = balanceConfig;
            _iconCatalog = iconCatalog;
        }

        public void RegisterToastView(IEventToastView view)
        {
            if (view != null)
                _toastController.RegisterView(view);
        }

        public override ScreenId Id => ScreenId.Hud;

        public Observable<HudViewState> State => _model.State;
        public Observable<HudResourceViewState> Resources => _model.ResourceState;
        public int CurrentDay => _dayTracker.CurrentDay;
        public ResourceIconCatalog ResourceIcons => _iconCatalog;

        protected override void OnOpen(object args)
        {
            _model.Activate();
            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        protected override void OnClose()
        {
            _model.Deactivate();
            _dayTracker.OnDayChanged -= HandleDayChanged;
        }

        public override void OnFocusGained()
        {
            VisibilityChanged?.Invoke(true);
            InteractableChanged?.Invoke(true);
        }

        public override void OnFocusLost()
        {
            InteractableChanged?.Invoke(false);
            if (_screenStackService.TopId == ScreenId.TargetSelection)
                VisibilityChanged?.Invoke(false);
        }

        public void OnStartAction()
        {
            switch (_model.CurrentMode)
            {
                case HudMode.Travel:
                    _model.SetSpeed(0);
                    break;
                case HudMode.City:
                    if (_cityEntryService.IsInCityView)
                        StartMove();
                    else
                        EnterCity();
                    break;
            }
        }

        public void OnAction()
        {
            if (_model.CurrentMode == HudMode.Travel)
                _model.SetSpeed(1);
        }

        public void OnEndAction()
        {
            switch (_model.CurrentMode)
            {
                case HudMode.Travel:
                    _model.SetSpeed(2);
                    break;
                case HudMode.City:
                    StartMove();
                    break;
            }
        }

        public void OpenInventory()
        {
            if (!_screenStackService.TryOpen(ScreenId.Inventory, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open inventory screen: {result}");
        }

        private void EnterCity()
        {
            if (_turnChoiceState.IsChoosingTurn)
            {
                _arrowsController.HideArrows();
                string turnNodeId = _turnChoiceState.CurrentTurnNodeId;
                if (!string.IsNullOrWhiteSpace(turnNodeId))
                    _playerMovementControl.CancelDestinationAtNode(turnNodeId);
            }

            if (!_model.TryGetEnterCity(out CityData city))
                return;

            if (_cityEntryService.CanEnterCity(city))
            {
                _cityEntryService.EnterCity(city);
            }
            else
            {
                _screenStackService.TryOpen(ScreenId.Trade, city.Id, out _);
            }
        }

        private void HandleDayChanged(int day) => DayChanged?.Invoke(day);

        private void StartMove()
        {
            if (_cityEntryService.IsInCityView)
            {
                _cityEntryService.ExitCity(() =>
                {
                    _screenStackService.TryOpen(ScreenId.TargetSelection, out _);
                });
                return;
            }

            _screenStackService.TryOpen(ScreenId.TargetSelection, out _);
        }
    }
}
