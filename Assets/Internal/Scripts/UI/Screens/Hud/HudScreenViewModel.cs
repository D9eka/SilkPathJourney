using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Hud;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.ViewModel;
using Internal.Scripts.UI.StackService;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Hud
{
    public sealed class HudScreenViewModel : ScreenViewModelBase
    {
        private readonly HudModel _model;
        private readonly ScreenStackService _screenStackService;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerStartMovement _playerStartMovement;
        private readonly IPlayerMovementControl _playerMovementControl;
        private readonly IPlayerTurnChoiceState _turnChoiceState;
        private readonly IArrowsController _arrowsController;

        public event Action<bool> InteractableChanged;

        public HudScreenViewModel(
            HudModel model,
            ScreenStackService screenStackService,
            IPlayerStateProvider playerStateProvider,
            IPlayerStartMovement playerStartMovement,
            IPlayerMovementControl playerMovementControl,
            IPlayerTurnChoiceState turnChoiceState,
            IArrowsController arrowsController)
        {
            _model = model;
            _screenStackService = screenStackService;
            _playerStateProvider = playerStateProvider;
            _playerStartMovement = playerStartMovement;
            _playerMovementControl = playerMovementControl;
            _turnChoiceState = turnChoiceState;
            _arrowsController = arrowsController;
        }

        public override ScreenId Id => ScreenId.Hud;

        public Observable<HudViewState> State => _model.State;

        protected override void OnOpen(object args)
        {
            _model.Activate();
        }

        protected override void OnClose()
        {
            _model.Deactivate();
        }

        public override void OnFocusGained()
        {
            InteractableChanged?.Invoke(true);
        }

        public override void OnFocusLost()
        {
            InteractableChanged?.Invoke(false);
        }

        public void OpenInventory()
        {
            if (!_screenStackService.TryOpen(ScreenId.Inventory, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open inventory screen: {result}");
        }

        public void EnterCity()
        {
            string nodeId = _playerStateProvider.CurrentNodeId;
            if (_turnChoiceState.IsChoosingTurn)
            {
                _arrowsController.HideArrows();
                string turnNodeId = _turnChoiceState.CurrentTurnNodeId;
                if (!string.IsNullOrWhiteSpace(turnNodeId))
                {
                    _playerMovementControl.CancelDestinationAtNode(turnNodeId);
                    nodeId = turnNodeId;
                }
            }

            if (_model.TryGetEnterCity(out CityData city))
            {
                if (!_screenStackService.TryOpen(ScreenId.Trade, city.Id, out ScreenOpenResult result))
                    Debug.LogWarning($"[SPJ] Cannot open trade screen: {result}");
                return;
            }

            Debug.LogWarning($"[SPJ] Cannot enter city: no city bound to node '{nodeId}'.");
        }

        public void StartMove()
        {
            _playerStartMovement.BeginSelection();
        }

        public void CancelMove()
        {
            _playerStartMovement.CancelSelection();
        }
    }
}
