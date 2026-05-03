using System;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Config;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player;

namespace Internal.Scripts.Economy.Buildings.Healer
{
    public sealed class HealerService
    {
        public event Action<CompanionState, int> OnHealed;

        private readonly CompanionService _companionService;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly GameBalanceConfig _balance;

        public HealerService(CompanionService companionService, PlayerResourceRepository resourceRepository, GameBalanceConfig balance)
        {
            _companionService = companionService;
            _resourceRepository = resourceRepository;
            _balance = balance;
        }

        public int GetHealCost(CompanionState companion)
        {
            if (string.IsNullOrEmpty(companion.QualityId))
                return _balance.HealerNoviceCost;

            if (System.Enum.TryParse(companion.QualityId, true, out CompanionQuality quality))
            {
                return quality switch
                {
                    CompanionQuality.Master => _balance.HealerMasterCost,
                    CompanionQuality.Experienced => _balance.HealerExperiencedCost,
                    _ => _balance.HealerNoviceCost
                };
            }

            return _balance.HealerNoviceCost;
        }

        public bool TryHeal(int companionIndex)
        {
            var state = _resourceRepository.Current;
            if (companionIndex < 0 || companionIndex >= state.Companions.Count)
                return false;

            var companion = state.Companions[companionIndex];
            if (!companion.IsInjured)
                return false;

            int cost = GetHealCost(companion);
            if (state.Money < cost)
                return false;

            _resourceRepository.UpdateResources(s => s.Money -= cost);
            _companionService.HealCompanion(companionIndex);
            OnHealed?.Invoke(companion, cost);
            return true;
        }
    }
}
