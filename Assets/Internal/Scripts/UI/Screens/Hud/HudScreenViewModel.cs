using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.ViewModel;
using Internal.Scripts.UI.StackService;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Hud
{
    public sealed class HudScreenViewModel : ScreenViewModelBase
    {
        private readonly HudScreen _view;
        private readonly ScreenStackService _screenStackService;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerStateEvents _playerStateEvents;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IPlayerStartMovement _playerStartMovement;
        private readonly IPlayerMovementControl _playerMovementControl;
        private readonly IPlayerTurnChoiceState _turnChoiceState;
        private readonly IArrowsController _arrowsController;

        public HudScreenViewModel(
            HudScreen view,
            ScreenStackService screenStackService,
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents,
            ICityNodeResolver cityNodeResolver,
            IPlayerStartMovement playerStartMovement,
            IPlayerMovementControl playerMovementControl,
            IPlayerTurnChoiceState turnChoiceState,
            IArrowsController arrowsController) : base(view)
        {
            _view = view;
            _screenStackService = screenStackService;
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
            _cityNodeResolver = cityNodeResolver;
            _playerStartMovement = playerStartMovement;
            _playerMovementControl = playerMovementControl;
            _turnChoiceState = turnChoiceState;
            _arrowsController = arrowsController;
        }

        public override ScreenId Id => ScreenId.Hud;

        protected override void OnOpen(object args)
        {
            _view.BindOpenInventory(OpenInventory);
            _view.BindEnterCity(EnterCity);
            _view.BindStartMove(StartMove);
            _view.BindCancelMove(CancelMove);

            _playerStateEvents.OnCurrentNodeChanged += HandlePlayerStateChanged;
            _playerStateEvents.OnDestinationChanged += HandlePlayerStateChanged;
            _playerStartMovement.OnSelectionStateChanged += HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged += HandleTurnChoiceChanged;

            UpdateButtons();
        }

        protected override void OnClose()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandlePlayerStateChanged;
            _playerStateEvents.OnDestinationChanged -= HandlePlayerStateChanged;
            _playerStartMovement.OnSelectionStateChanged -= HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged -= HandleTurnChoiceChanged;

            _view.UnbindAll();
        }

        public override void OnFocusGained()
        {
            _view.SetInteractable(true);
            UpdateButtons();
        }

        public override void OnFocusLost()
        {
            _view.SetInteractable(false);
        }

        private void HandlePlayerStateChanged(string _)
        {
            UpdateButtons();
        }

        private void HandleSelectionChanged(bool _)
        {
            UpdateButtons();
        }

        private void HandleTurnChoiceChanged(bool _)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool isSelectingTarget = _playerStartMovement.IsChoosingTarget;
            bool isMoving = _playerStateProvider.State == PlayerState.Moving;

            _view.SetStartMoveVisible(!isSelectingTarget && !isMoving);
            _view.SetCancelMoveVisible(isSelectingTarget);
            _view.SetEnterCityVisible(CanEnterCity());
        }

        private void OpenInventory()
        {
            if (!_screenStackService.TryOpen(ScreenId.Inventory, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open inventory screen: {result}");
        }

        private void EnterCity()
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

            if (_cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city))
            {
                if (!_screenStackService.TryOpen(ScreenId.Trade, city.Id, out ScreenOpenResult result))
                {
                    Debug.LogWarning($"[SPJ] Cannot open trade screen: {result}");
                }
                return;
            }

            Debug.LogWarning($"[SPJ] Cannot enter city: no city bound to node '{nodeId}'.");
        }

        private void StartMove()
        {
            _playerStartMovement.BeginSelection();
        }

        private void CancelMove()
        {
            _playerStartMovement.CancelSelection();
        }

        private bool CanEnterCity()
        {
            PlayerState state = _playerStateProvider.State;
            if (state == PlayerState.SelectingTarget)
                return false;

            if (state != PlayerState.Idle &&
                !_turnChoiceState.IsChoosingTurn)
                return false;

            string nodeId = _playerStateProvider.CurrentNodeId;
            if (_turnChoiceState.IsChoosingTurn)
                nodeId = _turnChoiceState.CurrentTurnNodeId;

            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            return _cityNodeResolver.TryGetCityByNodeId(nodeId, out _);
        }
    }
}
