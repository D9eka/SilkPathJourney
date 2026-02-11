using UnityEngine;

namespace Internal.Scripts.World.State
{
    public class GameClock
    {
        public float TimeScale { get; private set; } = 1f;
        public float DeltaTime => Time.deltaTime * TimeScale;
        public bool IsPaused => TimeScale <= 0f;

        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Max(0f, scale);
        }

        public void Pause() => SetTimeScale(0f);
        public void Resume() => SetTimeScale(1f);
    }
}
