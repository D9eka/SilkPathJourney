using System;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Npc.Core;
using R3;

namespace Internal.Scripts.Player
{
    public sealed class CaravanSpeedService : IDisposable
    {
        private readonly CaravanSpeedConfig _config;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly OverloadCalculator _overload;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly PlayerResourceRepository _resourceRepo;

        private RoadAgent _agent;
        private IDisposable _subscription;

        public ReactiveProperty<CaravanSpeedMode> CurrentMode { get; } = new(CaravanSpeedMode.Normal);

        public CaravanSpeedService(
            CaravanSpeedConfig config,
            GameBalanceConfig balanceConfig,
            OverloadCalculator overload,
            CaravanDatabase caravanDatabase,
            PlayerResourceRepository resourceRepo)
        {
            _config = config;
            _balanceConfig = balanceConfig;
            _overload = overload;
            _caravanDatabase = caravanDatabase;
            _resourceRepo = resourceRepo;

            _subscription = _resourceRepo.StateStream.Subscribe(_ => ApplySpeed());
        }

        public void Initialize(RoadAgent agent)
        {
            _agent = agent;
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

            PlayerResourceState resources = _resourceRepo.Current;
            float baseSpeed = resources.PlayerCart.Speed;

            CartClassData classData = _caravanDatabase.GetCartClassById(resources.CartClassId);
            if (classData != null)
                baseSpeed = classData.SpeedKmDay;

            var upgradeLevel = _caravanDatabase.GetUpgradeLevelById(resources.CartUpgradeLevelId);
            baseSpeed *= upgradeLevel.SpeedMult;

            DraftAnimalData animal = _caravanDatabase.GetDraftAnimalById(resources.DraftAnimalId);
            if (animal != null)
                baseSpeed *= 1f + animal.SpeedModPct / 100f;

            float extraCartPenalty = 0f;
            if (resources.Carts != null)
            {
                foreach (CartState cart in resources.Carts)
                {
                    if (string.IsNullOrEmpty(cart.TypeId)) continue;
                    ExtraCartData extra = _caravanDatabase.ExtraCarts.Find(e => e.Id == cart.TypeId);
                    if (extra != null)
                        extraCartPenalty += extra.SpeedPenaltyPct;
                }
            }
            baseSpeed *= 1f - extraCartPenalty / 100f;

            SpeedModeData modeData = _config.GetModeData(CurrentMode.Value);
            _agent.SpeedMetersPerDay = baseSpeed * _balanceConfig.WorldUnitsPerKm * modeData.SpeedMultiplier * _overload.GetSpeedModifier();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            CurrentMode?.Dispose();
        }
    }
}
