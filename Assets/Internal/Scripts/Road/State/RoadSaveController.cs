using System;
using Internal.Scripts.Save;
using Zenject;

namespace Internal.Scripts.Road.State
{
    public sealed class RoadSaveController : IInitializable, IDisposable
    {
        private readonly RoadUnlockService _roadUnlockService;
        private readonly SaveRepository _saveRepository;

        public RoadSaveController(RoadUnlockService roadUnlockService, SaveRepository saveRepository)
        {
            _roadUnlockService = roadUnlockService;
            _saveRepository = saveRepository;
        }

        public void Initialize()
        {
            _roadUnlockService.OnRoadUnlocked += HandleRoadUnlocked;
        }

        public void Dispose()
        {
            _roadUnlockService.OnRoadUnlocked -= HandleRoadUnlocked;
        }

        private void HandleRoadUnlocked(string roadId)
        {
            SaveData data = _saveRepository.Data;
            data.Roads ??= new RoadSaveData();
            data.Roads.UnlockedRoadIds = _roadUnlockService.SaveState();
            _saveRepository.Save();
        }
    }
}
