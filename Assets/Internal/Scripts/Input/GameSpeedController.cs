using System;
using Internal.Scripts.World.State;
using Zenject;

namespace Internal.Scripts.Input
{
    public sealed class GameSpeedController : IInitializable, IDisposable
    {
        private readonly InputRouter _inputManager;
        private readonly GameClock _gameClock;

        private TimeSpeed _speedBeforePause = TimeSpeed.Normal;

        public GameSpeedController(InputRouter inputManager, GameClock gameClock)
        {
            _inputManager = inputManager;
            _gameClock = gameClock;
        }

        public void Initialize()
        {
            _inputManager.OnTimeSpeedPause += HandlePause;
            _inputManager.OnTimeSpeed1 += HandleSpeed1;
            _inputManager.OnTimeSpeed2 += HandleSpeed2;
            _inputManager.OnTimeSpeed3 += HandleSpeed3;
        }

        public void Dispose()
        {
            _inputManager.OnTimeSpeedPause -= HandlePause;
            _inputManager.OnTimeSpeed1 -= HandleSpeed1;
            _inputManager.OnTimeSpeed2 -= HandleSpeed2;
            _inputManager.OnTimeSpeed3 -= HandleSpeed3;
        }

        private void HandlePause()
        {
            if (_gameClock.SelectedSpeed.CurrentValue == TimeSpeed.Paused)
            {
                _gameClock.SetSelectedSpeed(_speedBeforePause);
            }
            else
            {
                _speedBeforePause = _gameClock.SelectedSpeed.CurrentValue;
                _gameClock.SetSelectedSpeed(TimeSpeed.Paused);
            }
        }

        private void HandleSpeed1() => SetSpeed(TimeSpeed.Normal);
        private void HandleSpeed2() => SetSpeed(TimeSpeed.Fast);
        private void HandleSpeed3() => SetSpeed(TimeSpeed.VeryFast);

        private void SetSpeed(TimeSpeed speed)
        {
            _speedBeforePause = speed;
            _gameClock.SetSelectedSpeed(speed);
        }
    }
}
