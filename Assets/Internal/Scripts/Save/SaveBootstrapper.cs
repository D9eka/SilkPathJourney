using Internal.Scripts.Config;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Player;
using Internal.Scripts.Road.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Save
{
    public sealed class SaveBootstrapper : IInitializable
    {
        private readonly SaveRepository _saveRepository;
        private readonly EconomySaveBuilder _economySaveBuilder;
        private readonly PlayerConfig _playerConfig;
        private readonly RoadUnlockService _roadUnlockService;
        private readonly GameBalanceConfig _balanceConfig;

        public SaveBootstrapper(
            SaveRepository saveRepository,
            EconomySaveBuilder economySaveBuilder,
            PlayerConfig playerConfig,
            RoadUnlockService roadUnlockService,
            GameBalanceConfig balanceConfig)
        {
            _saveRepository = saveRepository;
            _economySaveBuilder = economySaveBuilder;
            _playerConfig = playerConfig;
            _roadUnlockService = roadUnlockService;
            _balanceConfig = balanceConfig;
        }

        public void Initialize()
        {
            SaveData data = _saveRepository.Data;
            bool changed = false;

            if (data.Version < 2)
                changed |= MigrateToV2(data);

            if (data.Version < 3)
                changed |= MigrateToV3(data);

            if (data.Economy == null || !data.Economy.IsInitialized)
            {
                data.Economy = _economySaveBuilder.Build();
                changed = true;
            }

            if (data.Player == null || string.IsNullOrWhiteSpace(data.Player.CurrentNodeId))
            {
                if (data.Player == null)
                    data.Player = new PlayerSaveData();

                data.Player.CurrentNodeId = ResolveStartNodeId();
                data.Player.DestinationNodeId = string.Empty;
                data.Player.State = PlayerState.Idle;
                changed = true;
            }

            if (data.Roads != null)
                _roadUnlockService.LoadState(data.Roads.UnlockedRoadIds);

            if (changed)
                _saveRepository.Save();
        }

        private bool MigrateToV2(SaveData data)
        {
            float secondsPerDay = _balanceConfig != null
                ? Mathf.Max(0f, _balanceConfig.SecondsPerDay)
                : 0f;

            if (secondsPerDay > 0f)
            {
                PlayerResourceState resources = data.Economy?.PlayerResources;
                if (resources?.PlayerCart != null)
                {
                    resources.PlayerCart.Speed *= secondsPerDay;
                }

                if (resources?.Carts != null)
                {
                    foreach (CartState cart in resources.Carts)
                        cart.Speed *= secondsPerDay;
                }
            }

            data.Version = 2;
            return true;
        }

        private bool MigrateToV3(SaveData data)
        {
            data.Npcs ??= new NpcSaveData();
            data.Version = 3;
            return true;
        }

        private string ResolveStartNodeId()
        {
            if (_playerConfig != null && !string.IsNullOrWhiteSpace(_playerConfig.StartNodeId))
                return _playerConfig.StartNodeId;

            return "N_Quanzhou";
        }
    }
}
