namespace Internal.Scripts.Trading
{
    public readonly struct PriceBreakdown
    {
        public readonly string ItemName;
        public readonly int BasePrice;
        public readonly float MarketMult;
        public readonly float BonusMult;
        public readonly int FinalPrice;
        public readonly bool IsNpcTrade;

        public PriceBreakdown(string itemName, int basePrice, float marketMult, float bonusMult, int finalPrice, bool isNpcTrade)
        {
            ItemName = itemName;
            BasePrice = basePrice;
            MarketMult = marketMult;
            BonusMult = bonusMult;
            FinalPrice = finalPrice;
            IsNpcTrade = isNpcTrade;
        }
    }
}
