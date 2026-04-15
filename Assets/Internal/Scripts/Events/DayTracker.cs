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
        private int _targetDay = -1;
        private float _savedTimeScale;

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

        public void AdvanceDaysAnimated(int count, float speedMultiplier = 30f)
        {
            if (IsSkipping) return;
            _targetDay = CurrentDay + count;
            _savedTimeScale = _gameClock.TimeScale;
            _gameClock.SetTimeScale(speedMultiplier);
            IsSkipping = true;
        }

        public void FixedTick()
        {
            if (!_worldState.IsActive && !IsSkipping) return;

            _accumulatedTime += Time.fixedDeltaTime * _gameClock.TimeScale;

            while (_accumulatedTime >= _balanceConfig.SecondsPerDay)
            {
                _accumulatedTime -= _balanceConfig.SecondsPerDay;
                _saveRepository.Data.Player.CurrentDay++;
                OnDayChanged?.Invoke(CurrentDay);
                _saveRepository.Save();
                if (IsSkipping && CurrentDay >= _targetDay)
                {
                    _gameClock.SetTimeScale(_savedTimeScale);
                    IsSkipping = false;
                    _targetDay = -1;
                    _accumulatedTime = 0f;
                    break;
                }
            }
        }
    }
}
