using Internal.Scripts.Config;
using UnityEngine;

namespace Internal.Scripts.World.State
{
    public sealed class GameDayDeltaProvider : IGameDayDeltaProvider
    {
        private readonly GameClock _gameClock;
        private readonly GameBalanceConfig _balanceConfig;

        public GameDayDeltaProvider(GameClock gameClock, GameBalanceConfig balanceConfig)
        {
            _gameClock = gameClock;
            _balanceConfig = balanceConfig;
        }

        public float GetFrameDayDelta()
        {
            float secondsPerDay = _balanceConfig.SecondsPerDay;
            if (secondsPerDay <= 0f)
                return 0f;

            return (Time.deltaTime * _gameClock.TimeScale) / secondsPerDay;
        }
    }
}
