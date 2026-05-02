using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.City
{
    public sealed class PlayerCityButtonsController : IInitializable, ITickable, IDisposable
    {
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IPlayerStartMovement _playerStartMovement;
        private readonly IPlayerMovementControl _playerMovementControl;
        private readonly IPlayerTurnChoiceState _turnChoiceState;
        private readonly IArrowsController _arrowsController;
        private readonly ScreenStackService _screenStackService;
        private readonly ICityEntryService _cityEntryService;
        private readonly Button _enterCityButton;

        private bool _lastCanEnter;

        public PlayerCityButtonsController(
            IPlayerStateProvider playerStateProvider,
            ICityNodeResolver cityNodeResolver,
            IPlayerStartMovement playerStartMovement,
            IPlayerMovementControl playerMovementControl,
            IPlayerTurnChoiceState turnChoiceState,
            IArrowsController arrowsController,
            ScreenStackService screenStackService,
            ICityEntryService cityEntryService,
            Button enterCityButton)
        {
            _playerStateProvider = playerStateProvider;
            _cityNodeResolver = cityNodeResolver;
            _playerStartMovement = playerStartMovement;
            _playerMovementControl = playerMovementControl;
            _turnChoiceState = turnChoiceState;
            _arrowsController = arrowsController;
            _screenStackService = screenStackService;
            _cityEntryService = cityEntryService;
            _enterCityButton = enterCityButton;
        }

        public void Initialize()
        {
            _enterCityButton.onClick.AddListener(OnEnterCity);
            _cityEntryService.OnCityAutoApproached += HandleCityAutoApproached;

            UpdateButtons(force: true);
        }

        public void Dispose()
        {
            _enterCityButton.onClick.RemoveListener(OnEnterCity);
            _cityEntryService.OnCityAutoApproached -= HandleCityAutoApproached;
        }

        public void Tick()
        {
            UpdateButtons(force: false);
        }

        private void UpdateButtons(bool force)
        {
            bool canEnter = CanEnterCity();

            if (!force && canEnter == _lastCanEnter)
                return;

            _lastCanEnter = canEnter;
            
            _enterCityButton.gameObject.SetActive(canEnter);
        }

        private void OnEnterCity()
        {
            string nodeId = _playerStateProvider.CurrentNodeId;
            if (_turnChoiceState != null && _turnChoiceState.IsChoosingTurn)
            {
                _arrowsController?.HideArrows();
                string turnNodeId = _turnChoiceState.CurrentTurnNodeId;
                if (!string.IsNullOrWhiteSpace(turnNodeId))
                {
                    _playerMovementControl?.CancelDestinationAtNode(turnNodeId);
                    nodeId = turnNodeId;
                }
            }

            if (_cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city))
            {
                OpenCityEntryConfirm(city);
                return;
            }

            Debug.LogWarning($"[SPJ] Cannot enter city: no city bound to node '{nodeId}'.");
        }

        private bool CanEnterCity()
        {
            PlayerState state = _playerStateProvider.State;
            if (state == PlayerState.SelectingTarget)
                return false;

            if (state != PlayerState.Idle &&
                (_turnChoiceState == null || !_turnChoiceState.IsChoosingTurn))
                return false;

            string nodeId = _playerStateProvider.CurrentNodeId;
            if (_turnChoiceState != null && _turnChoiceState.IsChoosingTurn)
                nodeId = _turnChoiceState.CurrentTurnNodeId;

            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            return _cityNodeResolver.TryGetCityByNodeId(nodeId, out _);
        }

        private void HandleCityAutoApproached(CityData city) => OpenCityEntryConfirm(city);

        private void OpenCityEntryConfirm(CityData city)
        {
            if (!_screenStackService.TryOpen(ScreenId.CityEntryConfirm, city, out ScreenOpenResult result)
                && result != ScreenOpenResult.AlreadyOpen)
                Debug.LogWarning($"[SPJ] Cannot open city entry screen: {result}");
        }
    }
}
