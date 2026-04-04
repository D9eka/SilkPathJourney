using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessNpcStatistics
    {
        public int TotalArrivals { get; private set; }
        public int TotalTrades { get; private set; }
        public int TotalSuppliesBoughtUnits { get; private set; }
        public int TotalGoodsBoughtUnits { get; private set; }
        public int TotalGoodsSoldUnits { get; private set; }
        public int TotalMoneySpentOnSupplies { get; private set; }
        public int TotalMoneySpentOnGoods { get; private set; }
        public int TotalMoneyEarnedFromGoods { get; private set; }
        public int TotalContractsCompleted { get; private set; }
        public int TotalContractRewards { get; private set; }
        public int TotalItemsBought => TotalSuppliesBoughtUnits + TotalGoodsBoughtUnits;
        public int TotalItemsSold => TotalGoodsSoldUnits;
        public int TotalMoneyEarned => TotalMoneyEarnedFromGoods;
        public int TotalMoneySpent => TotalMoneySpentOnSupplies + TotalMoneySpentOnGoods;

        public void RecordArrival() => TotalArrivals++;

        public void RecordTrade(NpcTrader.TradeExecutionStats stats)
        {
            if (stats.BoughtUnits > 0 || stats.SoldUnits > 0)
                TotalTrades++;
            TotalSuppliesBoughtUnits += stats.SuppliesBoughtUnits;
            TotalGoodsBoughtUnits += stats.GoodsBoughtUnits;
            TotalGoodsSoldUnits += stats.GoodsSoldUnits;
            TotalMoneySpentOnSupplies += stats.MoneySpentOnSupplies;
            TotalMoneySpentOnGoods += stats.MoneySpentOnGoods;
            TotalMoneyEarnedFromGoods += stats.MoneyEarnedFromGoods;
        }

        public void RecordContract(int reward)
        {
            TotalContractsCompleted++;
            TotalContractRewards += reward;
        }
    }
}
