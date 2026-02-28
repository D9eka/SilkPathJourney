using Internal.Scripts.Config;
using Internal.Scripts.Npc.Core;
using R3;

namespace Internal.Scripts.Player
{
    public sealed class CaravanSpeedService
    {
        private readonly CaravanSpeedConfig _config;
        private readonly OverloadCalculator _overload;

        private RoadAgent _agent;
        private float _baseSpeedMetersPerDay;

        public ReactiveProperty<CaravanSpeedMode> CurrentMode { get; } = new(CaravanSpeedMode.Normal);

        public CaravanSpeedService(CaravanSpeedConfig config, OverloadCalculator overload)
        {
            _config = config;
            _overload = overload;
        }

        public void Initialize(RoadAgent agent)
        {
            _agent = agent;
            _baseSpeedMetersPerDay = agent.SpeedMetersPerDay;
            ApplySpeed();
        }

        public void SetMode(CaravanSpeedMode mode)
        {
            CurrentMode.Value = mode;
            ApplySpeed();
        }

        public void RefreshSpeed() => ApplySpeed();

        private void ApplySpeed()
        {
            if (_agent == null) return;
            SpeedModeData data = _config.GetModeData(CurrentMode.Value);
            _agent.SpeedMetersPerDay = _baseSpeedMetersPerDay * data.SpeedMultiplier * _overload.GetSpeedModifier();
        }
    }
}
