using System.Collections.Generic;

namespace Internal.Scripts.Npc.Editor
{
    public struct DayPriceEntry
    {
        public int Day;
        public string CityId;
        public string ItemId;
        public int BuyPrice;
        public int SellPrice;
    }

    public struct RouteProfitEntry
    {
        public int Day;
        public string FromCity;
        public string ToCity;
        public string ItemId;
        public float ProfitPct;
    }

    public struct NpcSnapshot
    {
        public int CountBefore, CountAfter;
        public int TotalMoneyBefore, TotalMoneyAfter;
        public int TotalItemsBefore, TotalItemsAfter;
        public int SuppliesBoughtUnits, GoodsBoughtUnits, GoodsSoldUnits;
        public int MoneySpentOnSupplies, MoneySpentOnGoods, MoneyEarnedFromGoods;
        public int InventoryMarketValueBefore, InventoryMarketValueAfter;
        public float TotalDebtBefore, TotalDebtAfter;
        public float NetWealthBefore, NetWealthAfter;
    }

    public struct CityStockEntry
    {
        public int Day;
        public string CityId;
        public string ItemId;
        public int Stock;
    }

    public struct NpcStateEntry
    {
        public int Day;
        public string NpcName;
        public string Archetype;
        public string Experience;
        public int Money;
        public float Debt;
        public int ItemCount;
        public string CurrentCity;
        public string Destination;
    }

    public struct SimulationMetrics
    {
        public bool PricesStable;
        public bool ProfitInRange;
        public bool RoutesChange;
        public string Summary;
    }

    internal struct PriceStabilitySummary
    {
        public int StableCount;
        public int TotalCount;
        public float PassRate;
        public List<(string cityId, string itemId, float ratio)> WorstOutliers;
    }

    internal struct NpcMobilitySummary
    {
        public int SampledAgents;
        public int NoCityChanges;
        public int SingleRoutePair;
        public float AverageCityChanges;
        public float NoCityChangesRate => SampledAgents > 0 ? NoCityChanges / (float)SampledAgents : 0f;
        public float SingleRoutePairRate => SampledAgents > 0 ? SingleRoutePair / (float)SampledAgents : 0f;
    }
}
