using System;
using Internal.Scripts.Config;
using Internal.Scripts.Player;
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
        private readonly IPlayerStateProvider _playerState;

        private float _accumulatedTime;

        public int CurrentDay => _saveRepository.Data.Player.CurrentDay;

        public DayTracker(SaveRepository saveRepository, GameBalanceConfig balanceConfig,
            GameClock gameClock, IPlayerStateProvider playerState)
        {
            _saveRepository = saveRepository;
            _balanceConfig = balanceConfig;
            _gameClock = gameClock;
            _playerState = playerState;
        }

        public void FixedTick()
        {
            if (_gameClock.IsPaused) return;
            if (_playerState.State != PlayerState.Moving) return;

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
