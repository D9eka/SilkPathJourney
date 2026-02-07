using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class CartDurabilityOutcomeHandler : IOutcomeHandler
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.CartDurability
        };

        private readonly PlayerResourceRepository _resourceRepository;
        private readonly GameBalanceConfig _balanceConfig;

        public CartDurabilityOutcomeHandler(PlayerResourceRepository resourceRepository,
            GameBalanceConfig balanceConfig)
        {
            _resourceRepository = resourceRepository;
            _balanceConfig = balanceConfig;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            int cartIndex = string.IsNullOrEmpty(entry.Param) ? -1 : int.Parse(entry.Param);
            float durabilityChange = entry.Value;

            _resourceRepository.UpdateResources(s =>
            {
                if (cartIndex == -1)
                {
                    foreach (var cart in s.Carts)
                        cart.Durability = Mathf.Clamp(cart.Durability + durabilityChange, 0f, _balanceConfig.MaxCartDurability);
                }
                else if (cartIndex >= 0 && cartIndex < s.Carts.Count)
                {
                    s.Carts[cartIndex].Durability = Mathf.Clamp(
                        s.Carts[cartIndex].Durability + durabilityChange, 0f, _balanceConfig.MaxCartDurability);
                }
            });
        }
    }
}
