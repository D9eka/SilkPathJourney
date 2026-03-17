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

            int idx = _caravanDb.CaravanUpgrades.FindIndex(u => u.Type == type);
            if (idx < 0)
                return false;

            var upgradeEntry = _caravanDb.CaravanUpgrades[idx];
            if (state.Money < upgradeEntry.Price)
                return false;

            _resourceRepo.UpdateResources(s =>
            {
                s.Money -= upgradeEntry.Price;
                s.ActiveUpgrades.Add(upgradeId);
            });

            return true;
        }

        public bool HasUpgrade(CaravanUpgradeType type)
        {
            string upgradeId = type.ToString().ToLowerInvariant();
            return _resourceRepo.Current.ActiveUpgrades.Contains(upgradeId);
        }
    }
}
