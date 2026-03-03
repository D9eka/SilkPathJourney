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

        public int GetBuyPrice(string itemId)
        {
            int basePrice = _catalog.GetItemPrice(itemId);
            float mult = itemId == SuppliesItemId.Value
                ? _markup * _suppliesMarkupMultiplier
                : _markup;
            return Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f + mult)));
        }

        public int GetSellPrice(string itemId)
        {
            int basePrice = _catalog.GetItemPrice(itemId);
            if (itemId == SuppliesItemId.Value) return basePrice;
            return Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f - _markup)));
        }
    }
}
