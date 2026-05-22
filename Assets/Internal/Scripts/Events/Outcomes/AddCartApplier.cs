using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class AddCartApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddCart
        };

        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly QuestRepository _questRepository;

        public AddCartApplier(
            PlayerResourceRepository resourceRepository,
            CaravanDatabase caravanDatabase,
            GameBalanceConfig balanceConfig,
            QuestRepository questRepository)
        {
            _resourceRepository = resourceRepository;
            _caravanDatabase = caravanDatabase;
            _balanceConfig = balanceConfig;
            _questRepository = questRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        // Param format: "<cartTypeId>" or "<cartTypeId>|<questId>.<flagId>"
        // When at cap the cart is NOT added and the flag (if specified) is set to 1
        // to allow narrative branching ("convoy full, recruits walk").
        // IMPORTANT: cartTypeId used in add_cart/remove_cart outcomes must NOT match
        // any id from extra_carts.csv purchasable by the player, otherwise
        // RemoveCartApplier may remove a player-bought cart of the same TypeId.
        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Param))
            {
                Debug.LogWarning("[SPJ Events] AddCart outcome has empty Param (expected '<cartTypeId>' or '<cartTypeId>|<questId>.<flagId>').");
                return;
            }

            string typeId = entry.Param;
            string capFlagParam = null;

            int pipeIdx = entry.Param.IndexOf('|');
            if (pipeIdx >= 0)
            {
                typeId = entry.Param.Substring(0, pipeIdx);
                capFlagParam = entry.Param.Substring(pipeIdx + 1);
            }

            var cartData = _caravanDatabase.ExtraCarts.Find(e =>
                string.Equals(e.Id, typeId, System.StringComparison.OrdinalIgnoreCase));

            float durability = cartData != null ? cartData.Durability : 100f;
            float capacity = cartData != null ? cartData.Capacity : 50f;
            float speed = cartData != null ? cartData.SpeedPenaltyPct : 0f;
            int animalCount = cartData != null ? cartData.SuppliesPerDay : 1;

            bool atCap = false;
            _resourceRepository.UpdateResources(s =>
            {
                if (s.Carts.Count >= _balanceConfig.MaxExtraCarts)
                {
                    atCap = true;
                    return;
                }

                s.Carts.Add(new CartState
                {
                    TypeId = typeId,
                    Durability = durability,
                    MaxDurability = durability,
                    Capacity = capacity,
                    Speed = speed,
                    AnimalCount = animalCount
                });
            });

            if (atCap && !string.IsNullOrEmpty(capFlagParam))
            {
                int dotIdx = capFlagParam.IndexOf('.');
                if (dotIdx < 0)
                {
                    Debug.LogWarning($"[SPJ Events] AddCart cap-flag param '{capFlagParam}' must be 'questId.flagId'.");
                    return;
                }
                string questId = capFlagParam.Substring(0, dotIdx);
                string flagId = capFlagParam.Substring(dotIdx + 1);
                _questRepository.SetFlag(questId, flagId, 1);
            }
        }
    }
}
