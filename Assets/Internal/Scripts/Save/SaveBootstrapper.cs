using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player;
using Zenject;

namespace Internal.Scripts.Save
{
    public sealed class SaveBootstrapper : IInitializable
    {
        private readonly SaveRepository _saveRepository;
        private readonly EconomySaveBuilder _economySaveBuilder;
        private readonly PlayerConfig _playerConfig;

        public SaveBootstrapper(
            SaveRepository saveRepository,
            EconomySaveBuilder economySaveBuilder,
            PlayerConfig playerConfig)
        {
            _saveRepository = saveRepository;
            _economySaveBuilder = economySaveBuilder;
            _playerConfig = playerConfig;
        }

        public void Initialize()
        {
            SaveData data = _saveRepository.Data;
            bool changed = false;

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

            if (changed)
                _saveRepository.Save();
        }

        private string ResolveStartNodeId()
        {
            if (_playerConfig != null && !string.IsNullOrWhiteSpace(_playerConfig.StartNodeId))
                return _playerConfig.StartNodeId;

            return "N_Quanzhou";
        }
    }
}
