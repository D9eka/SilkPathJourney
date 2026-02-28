using Internal.Scripts.Config;
using R3;
using UnityEngine;

namespace Internal.Scripts.World.State
{
    public class GameClock
    {
        private readonly TimeSpeedConfig _config;
        private int _pauseStack;

        public GameClock(TimeSpeedConfig config)
        {
            _config = config;
        }

        public float TimeScale { get; private set; } = 1f;
        public float DeltaTime => Time.fixedDeltaTime * TimeScale;
        public bool IsPaused => TimeScale <= 0f;
        public ReactiveProperty<TimeSpeed> SelectedSpeed { get; } = new(TimeSpeed.Normal);

        public void SetSelectedSpeed(TimeSpeed speed)
        {
            SelectedSpeed.Value = speed;
            ApplyTimeScale();
        }

        public void Pause()
        {
            _pauseStack++;
            ApplyTimeScale();
        }

        public void Resume()
        {
            _pauseStack = Mathf.Max(0, _pauseStack - 1);
            ApplyTimeScale();
        }

        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Max(0f, scale);
        }

        private void ApplyTimeScale()
        {
            if (_pauseStack > 0)
            {
                TimeScale = 0f;
                return;
            }

            TimeScale = _config.GetMultiplier(SelectedSpeed.Value);
        }
    }
}
