using System;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player;

namespace Internal.Scripts.Economy.Buildings.Healer
{
    public sealed class HealerService
    {
        public event Action<CompanionState, int> OnHealed;
        private const int HEAL_COST_NOVICE = 10;
        private const int HEAL_COST_EXPERIENCED = 25;
        private const int HEAL_COST_MASTER = 50;

        private readonly CompanionService _companionService;
        private readonly PlayerResourceRepository _resourceRepository;

        public HealerService(CompanionService companionService, PlayerResourceRepository resourceRepository)
        {
            _companionService = companionService;
            _resourceRepository = resourceRepository;
        }

        public int GetHealCost(CompanionState companion)
        {
            if (string.IsNullOrEmpty(companion.QualityId))
                return HEAL_COST_NOVICE;

            if (System.Enum.TryParse(companion.QualityId, true, out CompanionQuality quality))
            {
                return quality switch
                {
                    CompanionQuality.Master => HEAL_COST_MASTER,
                    CompanionQuality.Experienced => HEAL_COST_EXPERIENCED,
                    _ => HEAL_COST_NOVICE
                };
            }

            return HEAL_COST_NOVICE;
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
