namespace Internal.Scripts.Trading
{
    public readonly struct PriceBreakdown
    {
        public readonly string ItemName;
        public readonly int BasePrice;
        public readonly float MarketMult;
        public readonly float BonusMult;
        public readonly float ModifierMult;
        public readonly float ExoticityMult;
        public readonly float TitheMult;
        public readonly int FinalPrice;
        public readonly bool IsNpcTrade;

        public PriceBreakdown(string itemName, int basePrice, float marketMult, float bonusMult, float modifierMult, float titheMult, int finalPrice, bool isNpcTrade)
        {
            ItemName = itemName;
            BasePrice = basePrice;
            MarketMult = marketMult;
            BonusMult = bonusMult;
            ModifierMult = modifierMult;
            ExoticityMult = 1f;
            TitheMult = titheMult;
            FinalPrice = finalPrice;
            IsNpcTrade = isNpcTrade;
        }

        public PriceBreakdown(string itemName, int basePrice, float marketMult,
            float bonusMult, float modifierMult, float exoticityMult, float titheMult, int finalPrice, bool isNpcTrade)
        {
            ItemName = itemName;
            BasePrice = basePrice;
            MarketMult = marketMult;
            BonusMult = bonusMult;
            ModifierMult = modifierMult;
            ExoticityMult = exoticityMult;
            TitheMult = titheMult;
            FinalPrice = finalPrice;
            IsNpcTrade = isNpcTrade;
        }
    }
}
