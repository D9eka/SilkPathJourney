using System;
using Internal.Scripts.Save;
using Zenject;

namespace Internal.Scripts.Player
{
    public sealed class PlayerSaveController : IInitializable, IDisposable
    {
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerStateEvents _playerStateEvents;
        private readonly SaveRepository _saveRepository;

        public PlayerSaveController(
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents,
            SaveRepository saveRepository)
        {
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
            _saveRepository = saveRepository;
        }

        public void Initialize()
        {
            PlayerSaveData playerSave = _saveRepository.Data.Player ?? new PlayerSaveData();
            _saveRepository.Data.Player = playerSave;

            _playerStateEvents.OnCurrentNodeChanged += HandleNodeChanged;
            _playerStateEvents.OnDestinationChanged += HandleDestinationChanged;
        }

        public void Dispose()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleNodeChanged;
            _playerStateEvents.OnDestinationChanged -= HandleDestinationChanged;
        }

        private void HandleNodeChanged(string nodeId)
        {
            Save();
        }

        private void HandleDestinationChanged(string destinationId)
        {
            Save();
        }

        private void Save()
        {
            PlayerSaveData playerSave = _saveRepository.Data.Player ?? new PlayerSaveData();
            _saveRepository.Data.Player = playerSave;
            playerSave.CurrentNodeId = _playerStateProvider.CurrentNodeId ?? string.Empty;
            playerSave.DestinationNodeId = _playerStateProvider.DestinationNodeId ?? string.Empty;
            playerSave.State = _playerStateProvider.State;
            _saveRepository.Save();
        }
    }
}
