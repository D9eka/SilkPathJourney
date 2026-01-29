using System;
using Internal.Scripts.Economy.Cities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.Player.UI.City
{
    public sealed class PlayerCityButtonsController : IInitializable, ITickable, IDisposable
    {
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly Button _enterCityButton;

        private bool _lastIsIdle;
        private bool _lastHasCity;

        public PlayerCityButtonsController(
            IPlayerStateProvider playerStateProvider,
            ICityNodeResolver cityNodeResolver,
            Button enterCityButton)
        {
            _playerStateProvider = playerStateProvider;
            _cityNodeResolver = cityNodeResolver;
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
            bool isIdle = _playerStateProvider.State == PlayerState.Idle;
            bool hasCity = isIdle && _cityNodeResolver.TryGetCityByNodeId(_playerStateProvider.CurrentNodeId, out _);

            if (!force && isIdle == _lastIsIdle && hasCity == _lastHasCity)
                return;

            _lastIsIdle = isIdle;
            _lastHasCity = hasCity;
            
            _enterCityButton.gameObject.SetActive(isIdle && hasCity);
        }

        private void OnEnterCity()
        {
            string nodeId = _playerStateProvider.CurrentNodeId;
            if (_cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city))
            {
                Debug.Log($"[SPJ] Enter city requested: {city.Id}");
                return;
            }

            Debug.LogWarning($"[SPJ] Cannot enter city: no city bound to node '{nodeId}'.");
        }
    }
}
