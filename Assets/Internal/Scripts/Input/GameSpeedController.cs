using System;
using Internal.Scripts.World.State;
using Zenject;

namespace Internal.Scripts.Input
{
    public sealed class GameSpeedController : IInitializable, IDisposable
    {
        private readonly ITimeSpeedInput _input;
        private readonly GameClock _gameClock;

        private TimeSpeed _speedBeforePause = TimeSpeed.Normal;

        public GameSpeedController(ITimeSpeedInput input, GameClock gameClock)
        {
            _input = input;
            _gameClock = gameClock;
        }

        public void Initialize()
        {
            _input.OnPause  += HandlePause;
            _input.OnSpeed1 += HandleSpeed1;
            _input.OnSpeed2 += HandleSpeed2;
            _input.OnSpeed3 += HandleSpeed3;
        }

        public void Dispose()
        {
            _input.OnPause  -= HandlePause;
            _input.OnSpeed1 -= HandleSpeed1;
            _input.OnSpeed2 -= HandleSpeed2;
            _input.OnSpeed3 -= HandleSpeed3;
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
