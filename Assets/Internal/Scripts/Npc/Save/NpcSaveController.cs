using System;
using Internal.Scripts.Events;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Save;
using Zenject;

namespace Internal.Scripts.Npc.Save
{
    public sealed class NpcSaveController : IInitializable, IDisposable
    {
        private readonly NpcLifeSimulator _lifeSimulator;
        private readonly SaveRepository _saveRepository;
        private readonly DayTracker _dayTracker;

        public NpcSaveController(NpcLifeSimulator lifeSimulator, SaveRepository saveRepository,
            DayTracker dayTracker)
        {
            _lifeSimulator = lifeSimulator;
            _saveRepository = saveRepository;
            _dayTracker = dayTracker;
        }

        public void Initialize()
        {
            _lifeSimulator.OnTradeCompleted += Save;
            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        public void Dispose()
        {
            _lifeSimulator.OnTradeCompleted -= Save;
            _dayTracker.OnDayChanged -= HandleDayChanged;
        }

        private void HandleDayChanged(int day) => Save();

        private void Save()
        {
            _saveRepository.Data.Npcs = _lifeSimulator.BuildSaveData();
            _saveRepository.Save();
        }
    }
}
