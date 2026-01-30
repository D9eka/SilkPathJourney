using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.StartMovement;
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
        private readonly Button _enterCityButton;

        private bool _lastCanEnter;

        public PlayerCityButtonsController(
            IPlayerStateProvider playerStateProvider,
            ICityNodeResolver cityNodeResolver,
            IPlayerStartMovement playerStartMovement,
            IPlayerMovementControl playerMovementControl,
            IPlayerTurnChoiceState turnChoiceState,
            IArrowsController arrowsController,
            Button enterCityButton)
        {
            _playerStateProvider = playerStateProvider;
            _cityNodeResolver = cityNodeResolver;
            _playerStartMovement = playerStartMovement;
            _playerMovementControl = playerMovementControl;
            _turnChoiceState = turnChoiceState;
            _arrowsController = arrowsController;
            _enterCityButton = enterCityButton;
        }

        public void Initialize()
        {
            _enterCityButton.onClick.AddListener(OnEnterCity);

            UpdateButtons(force: true);
        }

        public void Dispose()
        {
            _enterCityButton.onClick.RemoveListener(OnEnterCity);
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
                Debug.Log($"[SPJ] Enter city requested: {city.Id}");
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
    }
}
