using Internal.Scripts.Items;
using UnityEngine;

namespace Internal.Scripts.Trading
{
    public sealed class NpcTradePricing : ITradePricing
    {
        private readonly ItemCatalog _catalog;
        private readonly float _markup;
        private readonly float _suppliesMarkupMultiplier;

        public NpcTradePricing(ItemCatalog catalog, float markup, float suppliesMarkupMultiplier)
        {
            _catalog = catalog;
            _markup = markup;
            _suppliesMarkupMultiplier = suppliesMarkupMultiplier;
        }

        public int GetBuyPrice(string itemId) => CalculateBuy(itemId).finalPrice;

        public int GetSellPrice(string itemId) => CalculateSell(itemId).finalPrice;

        public PriceBreakdown GetBuyBreakdown(string itemId)
        {
            var (basePrice, marketMult, finalPrice) = CalculateBuy(itemId);
            return new PriceBreakdown(_catalog.ResolveItemName(itemId), basePrice, marketMult, 1f, 1f, 1f, finalPrice, true);
        }

        public PriceBreakdown GetSellBreakdown(string itemId)
        {
            var (basePrice, marketMult, finalPrice) = CalculateSell(itemId);
            return new PriceBreakdown(_catalog.ResolveItemName(itemId), basePrice, marketMult, 1f, 1f, 1f, finalPrice, true);
        }

        private (int basePrice, float marketMult, int finalPrice) CalculateBuy(string itemId)
        {
            int basePrice = _catalog.GetItemPrice(itemId);
            float mult = itemId == SuppliesItemId.Value ? _markup * _suppliesMarkupMultiplier : _markup;
            float marketMult = 1f + mult;
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * marketMult));
            return (basePrice, marketMult, finalPrice);
        }

        private (int basePrice, float marketMult, int finalPrice) CalculateSell(string itemId)
        {
            int basePrice = _catalog.GetItemPrice(itemId);
            float marketMult = itemId == SuppliesItemId.Value ? 1f : 1f - _markup;
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * marketMult));
            return (basePrice, marketMult, finalPrice);
        }
    }
}
