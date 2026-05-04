using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;

namespace Internal.Scripts.Player
{
    public sealed class CaravanUpgradeService
    {
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;

        public CaravanUpgradeService(PlayerResourceRepository resourceRepo, CaravanDatabase caravanDb)
        {
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
        }

        public bool PurchaseUpgrade(CaravanUpgradeType type)
        {
            string upgradeId = type.ToString().ToLowerInvariant();

            var state = _resourceRepo.Current;
            if (state.ActiveUpgrades.Contains(upgradeId))
                return false;

            if (!_caravanDb.CaravanUpgrades.Exists(u => u.Type == type))
                return false;

            int price = GetUpgradePrice(type);
            if (state.Money < price)
                return false;

            _resourceRepo.UpdateResources(s =>
            {
                s.Money -= price;
                s.ActiveUpgrades.Add(upgradeId);
            });

            return true;
        }

        public bool HasUpgrade(CaravanUpgradeType type)
        {
            string upgradeId = type.ToString().ToLowerInvariant();
            return _resourceRepo.Current.ActiveUpgrades.Contains(upgradeId);
        }

        public int GetUpgradePrice(CaravanUpgradeType type)
        {
            int idx = _caravanDb.CaravanUpgrades.FindIndex(u => u.Type == type);
            return idx >= 0 ? _caravanDb.CaravanUpgrades[idx].Price : 0;
        }
    }
}
