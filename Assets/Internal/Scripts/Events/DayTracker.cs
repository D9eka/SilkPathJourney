using System;
using Internal.Scripts.Config;
using Internal.Scripts.Save;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Events
{
    public class DayTracker : IFixedTickable
    {
        public event Action<int> OnDayChanged;

        private readonly SaveRepository _saveRepository;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly GameClock _gameClock;
        private readonly IWorldSimulationState _worldState;

        private float _accumulatedTime;

        public int CurrentDay => _saveRepository.Data.Player.CurrentDay;
        public bool IsSkipping { get; private set; }

        public DayTracker(SaveRepository saveRepository, GameBalanceConfig balanceConfig,
            GameClock gameClock, IWorldSimulationState worldState)
        {
            _saveRepository = saveRepository;
            _balanceConfig = balanceConfig;
            _gameClock = gameClock;
            _worldState = worldState;
        }

        public void AdvanceDays(int count)
        {
            IsSkipping = true;
            for (int i = 0; i < count; i++)
            {
                _saveRepository.Data.Player.CurrentDay++;
                OnDayChanged?.Invoke(CurrentDay);
            }
            IsSkipping = false;
            _saveRepository.Save();
        }

        public void FixedTick()
        {
            if (!_worldState.IsActive) return;

            _accumulatedTime += Time.fixedDeltaTime * _gameClock.TimeScale;

            while (_accumulatedTime >= _balanceConfig.SecondsPerDay)
            {
                _accumulatedTime -= _balanceConfig.SecondsPerDay;
                _saveRepository.Data.Player.CurrentDay++;
                _saveRepository.Save();
                OnDayChanged?.Invoke(CurrentDay);
            }
        }
    }
}
