using Internal.Scripts.Config;
using Internal.Scripts.Economy;

namespace Internal.Scripts.Economy.Buildings.Temple
{
    public sealed class TempleService
    {
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly GameBalanceConfig _balance;

        public TempleService(PlayerResourceRepository resourceRepository, GameBalanceConfig balance)
        {
            _resourceRepository = resourceRepository;
            _balance = balance;
        }

        public int GetPrayCost() => _balance.TemplePrayCost;
        public int GetDonateCost() => _balance.TempleDonateCost;
        public int GetBlessCost() => _balance.TempleBlessCost;

        public bool TryPray()
        {
            int cost = GetPrayCost();
            if (_resourceRepository.Current.Money < cost)
                return false;

            _resourceRepository.UpdateResources(s =>
            {
                s.Money -= cost;
                s.Morale += _balance.TempleMoraleGain;
            });
            return true;
        }

        public bool TryDonate()
        {
            int cost = GetDonateCost();
            if (_resourceRepository.Current.Money < cost)
                return false;

            _resourceRepository.UpdateResources(s =>
            {
                s.Money -= cost;
                s.Reputation += _balance.TempleReputationGain;
            });
            return true;
        }

        public bool TryBless()
        {
            int cost = GetBlessCost();
            if (_resourceRepository.Current.Money < cost)
                return false;

            _resourceRepository.UpdateResources(s =>
            {
                s.Money -= cost;
                s.AccumulatedDanger -= _balance.TempleDangerReduction;
                if (s.AccumulatedDanger < 0f) s.AccumulatedDanger = 0f;
            });
            return true;
        }
    }
}
