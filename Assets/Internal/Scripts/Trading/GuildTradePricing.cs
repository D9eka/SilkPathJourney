using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Items;
using UnityEngine;

namespace Internal.Scripts.Trading
{
    public sealed class GuildTradePricing : ITradePricing
    {
        private readonly ItemCatalog _itemCatalog;
        private readonly GuildSettings _guildSettings;
        private readonly GuildService _guildService;

        public GuildTradePricing(ItemCatalog itemCatalog, GuildSettings guildSettings, GuildService guildService)
        {
            _itemCatalog = itemCatalog;
            _guildSettings = guildSettings;
            _guildService = guildService;
        }

        public int GetBuyPrice(string itemId) => CalculateBuy(itemId).finalPrice;

        public int GetSellPrice(string itemId) => CalculateSell(itemId).finalPrice;

        public PriceBreakdown GetBuyBreakdown(string itemId)
        {
            var (basePrice, marketMult, finalPrice) = CalculateBuy(itemId);
            return new PriceBreakdown(_itemCatalog.ResolveItemName(itemId), basePrice, marketMult, 1f, 1f, 1f, finalPrice, true);
        }

        public PriceBreakdown GetSellBreakdown(string itemId)
        {
            var (basePrice, marketMult, finalPrice) = CalculateSell(itemId);
            return new PriceBreakdown(_itemCatalog.ResolveItemName(itemId), basePrice, marketMult, 1f, 1f, 1f, finalPrice, true);
        }

        private (int basePrice, float marketMult, int finalPrice) CalculateBuy(string itemId)
        {
            int basePrice = _itemCatalog.GetItemPrice(itemId);
            float marketMult = _guildService.IsMember ? _guildSettings.GuildBuyMultMember : _guildSettings.GuildBuyMultNonMember;
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * marketMult));
            return (basePrice, marketMult, finalPrice);
        }

        private (int basePrice, float marketMult, int finalPrice) CalculateSell(string itemId)
        {
            int basePrice = _itemCatalog.GetItemPrice(itemId);
            float marketMult = _guildService.IsMember ? _guildSettings.GuildSellMultMember : _guildSettings.GuildSellMultNonMember;
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * marketMult));
            return (basePrice, marketMult, finalPrice);
        }
    }
}
