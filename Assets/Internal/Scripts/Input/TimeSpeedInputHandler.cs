using System;
using Internal.Scripts.World.State;
using Zenject;

namespace Internal.Scripts.Input
{
    public sealed class TimeSpeedInputHandler : IInitializable, IDisposable
    {
        private readonly InputManager _inputManager;
        private readonly GameClock _gameClock;

        public TimeSpeedInputHandler(InputManager inputManager, GameClock gameClock)
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

        private void HandlePause() => _gameClock.SetSelectedSpeed(TimeSpeed.Paused);
        private void HandleSpeed1() => _gameClock.SetSelectedSpeed(TimeSpeed.Normal);
        private void HandleSpeed2() => _gameClock.SetSelectedSpeed(TimeSpeed.Fast);
        private void HandleSpeed3() => _gameClock.SetSelectedSpeed(TimeSpeed.VeryFast);
    }
}
