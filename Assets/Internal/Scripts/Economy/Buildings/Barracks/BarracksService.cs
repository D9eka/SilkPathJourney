using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Config;
using Internal.Scripts.Player;

namespace Internal.Scripts.Economy.Buildings.Barracks
{
    public sealed class BarracksService
    {
        private readonly CompanionService _companionService;
        private readonly CaravanUpgradeService _caravanUpgradeService;
        private readonly GameBalanceConfig _balance;

        public BarracksService(
            CompanionService companionService,
            CaravanUpgradeService caravanUpgradeService,
            GameBalanceConfig balance)
        {
            _companionService = companionService;
            _caravanUpgradeService = caravanUpgradeService;
            _balance = balance;
        }

        public int GetGuardCost() => _balance.BarracksGuardCost;
        public int GetEliteGuardCost() => _caravanUpgradeService.GetUpgradePrice(CaravanUpgradeType.EliteGuard);

        public bool TryHireGuard()
        {
            int cost = GetGuardCost();
            return _companionService.HireCompanion(CompanionType.Guard, CompanionQuality.Experienced, cost);
        }

        public bool TryUpgradeEliteGuard()
        {
            return _caravanUpgradeService.PurchaseUpgrade(CaravanUpgradeType.EliteGuard);
        }

        public bool IsEliteGuardOwned()
        {
            return _caravanUpgradeService.HasUpgrade(CaravanUpgradeType.EliteGuard);
        }
    }
}
