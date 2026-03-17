using System;
using System.Linq;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;

namespace Internal.Scripts.Player
{
    public sealed class CompanionBonuses
    {
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;

        public CompanionBonuses(PlayerResourceRepository resourceRepo, CaravanDatabase caravanDb)
        {
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
        }

        public float GetActiveBonus(string bonusKey)
        {
            var companions = _resourceRepo.Current.Companions;
            if (companions == null || companions.Count == 0)
                return 0f;

            float maxBonus = 0f;

            foreach (var companion in companions)
            {
                if (companion.IsInjured)
                    continue;

                if (!Enum.TryParse(companion.TypeId, true, out CompanionType type))
                    continue;

                if (!Enum.TryParse(companion.QualityId, true, out CompanionQuality quality))
                    continue;

                var bonus = _caravanDb.GetCompanionBonus(type, quality);
                if (string.Equals(bonus.BonusKey, bonusKey, StringComparison.OrdinalIgnoreCase)
                    && bonus.BonusValue > maxBonus)
                {
                    maxBonus = bonus.BonusValue;
                }
            }

            return maxBonus;
        }

        public bool HasCompanionOfType(CompanionType type)
        {
            var companions = _resourceRepo.Current.Companions;
            if (companions == null)
                return false;

            string typeId = type.ToString();
            return companions.Any(c =>
                string.Equals(c.TypeId, typeId, StringComparison.OrdinalIgnoreCase) && !c.IsInjured);
        }
    }
}
