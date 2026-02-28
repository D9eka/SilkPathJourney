namespace Internal.Scripts.Economy.Simulation
{
    public readonly struct CityItemMarketProfile
    {
        public float TargetStock { get; }
        public float DailyNet { get; }
        public float EquilibriumPull { get; }
        public float BuyCoef { get; }
        public float SellCoef { get; }
        public bool HasDynamicMarket { get; }

        public CityItemMarketProfile(
            float targetStock, float dailyNet, float equilibriumPull,
            float buyCoef, float sellCoef, bool hasDynamicMarket)
        {
            TargetStock = targetStock;
            DailyNet = dailyNet;
            EquilibriumPull = equilibriumPull;
            BuyCoef = buyCoef;
            SellCoef = sellCoef;
            HasDynamicMarket = hasDynamicMarket;
        }

        public static CityItemMarketProfile Fallback => new(0f, 0f, 0f, 1f, 1f, false);
    }
}
