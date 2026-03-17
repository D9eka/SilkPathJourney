using System;
using System.Linq;
using Internal.Scripts.Caravan;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class CompanionCosts
    {
        private const string HEALER_TYPE_ID = "healer";

        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;

        public CompanionCosts(PlayerResourceRepository resourceRepo, CaravanDatabase caravanDb)
        {
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
        }

        public int CalculateDailyCost(CompanionState companion)
        {
            CompanionTypeData typeData = _caravanDb.GetCompanionTypeById(companion.TypeId);
            if (typeData == null) return 0;

            var quality = _caravanDb.GetCompanionQualityById(companion.QualityId);
            return Mathf.RoundToInt(typeData.DailyCostBase * quality.DailyCostMultiplier);
        }

        public void ProcessDailyPay(PlayerResourceState state)
        {
            if (state.Companions == null || state.Companions.Count == 0)
                return;

            for (int i = state.Companions.Count - 1; i >= 0; i--)
            {
                int dailyCost = CalculateDailyCost(state.Companions[i]);

                if (state.Money >= dailyCost)
                    state.Money -= dailyCost;
                else
                    state.Companions.RemoveAt(i);
            }
        }

        public void ProcessHealing()
        {
            _resourceRepo.UpdateResources(s =>
            {
                if (s.Companions == null || s.Companions.Count == 0)
                    return;

                bool hasHealer = s.Companions.Any(c =>
                    string.Equals(c.TypeId, HEALER_TYPE_ID, StringComparison.OrdinalIgnoreCase) && !c.IsInjured);

                if (!hasHealer)
                    return;

                foreach (var companion in s.Companions)
                {
                    if (companion.IsInjured)
                        companion.IsInjured = false;
                }
            });
        }
    }
}
