using System.Collections.Generic;
using System.Linq;

namespace Internal.Scripts.Npc.Editor
{
    internal static class SimulationMetricsCalculator
    {
        private const float PriceCorridorMax = 1.35f;
        private const float PriceStabilityPassRateMin = 0.80f;
        private const float ProfitTargetMin = 15f;
        private const float ProfitTargetMax = 25f;

        public static SimulationMetrics CalculateMetrics(
            IReadOnlyList<DayPriceEntry> priceHistory,
            IReadOnlyList<RouteProfitEntry> routeProfitability,
            IReadOnlyList<NpcStateEntry> npcStates,
            NpcSnapshot npcSnapshot,
            (string from, string to)[] routes)
        {
            if (priceHistory.Count == 0)
                return new SimulationMetrics
                {
                    PricesStable = false,
                    ProfitInRange = false,
                    RoutesChange = false,
                    Summary = "No data collected."
                };

            PriceStabilitySummary priceStability = CalculatePriceStability(priceHistory);
            bool pricesStable = priceStability.PassRate >= PriceStabilityPassRateMin;

            var profitValues = routeProfitability
                .Where(r => r.ProfitPct > 0f && SimulationRouteAnalyzer.IsRouteAnalysisItem(r.ItemId))
                .Select(r => r.ProfitPct)
                .ToList();
            float avgProfit = profitValues.Count > 0 ? profitValues.Average() : 0f;
            bool profitInRange = avgProfit >= ProfitTargetMin && avgProfit <= ProfitTargetMax;

            bool routesChange = CheckRoutesChange(routeProfitability);

            string summary = BuildSummary(priceHistory, routeProfitability, npcStates, npcSnapshot, routes,
                priceStability, pricesStable, avgProfit, profitInRange, routesChange);

            return new SimulationMetrics
            {
                PricesStable = pricesStable,
                ProfitInRange = profitInRange,
                RoutesChange = routesChange,
                Summary = summary
            };
        }

        public static PriceStabilitySummary CalculatePriceStability(IReadOnlyList<DayPriceEntry> priceHistory)
        {
            var groups = priceHistory.GroupBy(e => (e.CityId, e.ItemId)).ToList();
            int total = 0;
            int stable = 0;
            var worstOutliers = new List<(string cityId, string itemId, float ratio)>();

            foreach (var group in groups)
            {
                float min = group.Min(e => e.BuyPrice);
                float max = group.Max(e => e.BuyPrice);
                if (max <= 0f)
                    continue;

                total++;
                float ratio = min > 0f ? max / min : float.PositiveInfinity;
                bool inCorridor = min > 0f && ratio <= PriceCorridorMax;

                if (inCorridor)
                    stable++;
                else
                    worstOutliers.Add((group.Key.CityId, group.Key.ItemId, ratio));
            }

            worstOutliers.Sort((a, b) => b.ratio.CompareTo(a.ratio));

            return new PriceStabilitySummary
            {
                StableCount = stable,
                TotalCount = total,
                PassRate = total > 0 ? stable / (float)total : 0f,
                WorstOutliers = worstOutliers
            };
        }

        public static bool CheckRoutesChange(IReadOnlyList<RouteProfitEntry> routeProfitability)
        {
            if (routeProfitability.Count == 0) return false;

            int[] days = routeProfitability.Select(r => r.Day).Distinct().OrderBy(d => d).ToArray();
            if (days.Length < 2) return false;

            int firstDay = days[0];
            int lastDay = days[days.Length - 1];

            string BestRouteForDay(int day)
            {
                return routeProfitability
                    .Where(r => r.Day == day && SimulationRouteAnalyzer.IsRouteAnalysisItem(r.ItemId))
                    .OrderByDescending(r => r.ProfitPct)
                    .ThenBy(r => r.FromCity, System.StringComparer.Ordinal)
                    .ThenBy(r => r.ToCity, System.StringComparer.Ordinal)
                    .ThenBy(r => r.ItemId, System.StringComparer.Ordinal)
                    .Select(r => $"{r.FromCity}->{r.ToCity}:{r.ItemId}")
                    .FirstOrDefault();
            }

            string first = BestRouteForDay(firstDay);
            string last = BestRouteForDay(lastDay);
            return first != last;
        }

        public static NpcMobilitySummary CalculateNpcMobility(IReadOnlyList<NpcStateEntry> npcStates)
        {
            if (npcStates.Count == 0)
                return default;

            int sampledAgents = 0;
            int noCityChanges = 0;
            int singleRoutePair = 0;
            int totalCityChanges = 0;

            foreach (var npcGroup in npcStates.GroupBy(e => e.NpcName))
            {
                var orderedStates = npcGroup.OrderBy(e => e.Day).ToList();
                if (orderedStates.Count == 0)
                    continue;

                sampledAgents++;
                int cityChanges = 0;
                string previousCity = orderedStates[0].CurrentCity;
                var routePairs = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);

                foreach (NpcStateEntry state in orderedStates)
                {
                    routePairs.Add($"{state.CurrentCity}->{state.Destination}");
                    if (state.CurrentCity != previousCity)
                    {
                        cityChanges++;
                        previousCity = state.CurrentCity;
                    }
                }

                totalCityChanges += cityChanges;
                if (cityChanges == 0)
                    noCityChanges++;
                if (routePairs.Count <= 1)
                    singleRoutePair++;
            }

            return new NpcMobilitySummary
            {
                SampledAgents = sampledAgents,
                NoCityChanges = noCityChanges,
                SingleRoutePair = singleRoutePair,
                AverageCityChanges = sampledAgents > 0 ? totalCityChanges / (float)sampledAgents : 0f
            };
        }

        public static string BuildSummary(
            IReadOnlyList<DayPriceEntry> priceHistory,
            IReadOnlyList<RouteProfitEntry> routeProfitability,
            IReadOnlyList<NpcStateEntry> npcStates,
            NpcSnapshot npcSnapshot,
            (string from, string to)[] routes,
            PriceStabilitySummary priceStability,
            bool pricesStable, float avgProfit, bool profitInRange, bool routesChange)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Days simulated: {priceHistory.Select(e => e.Day).Distinct().Count()}");
            sb.AppendLine($"Price records: {priceHistory.Count}");
            sb.AppendLine($"Prices stable over time: {pricesStable} ({priceStability.StableCount}/{priceStability.TotalCount}, pass rate {priceStability.PassRate:P0}, target {PriceStabilityPassRateMin:P0})");
            sb.AppendLine($"Avg profitable route margin: {avgProfit:F1}% (target {ProfitTargetMin:F0}-{ProfitTargetMax:F0}%): {(profitInRange ? "OK" : "FAIL")}");
            sb.AppendLine($"Optimal route changes over time: {routesChange}");

            sb.AppendLine();
            sb.AppendLine("--- Worst city/item price swings ---");
            if (priceStability.WorstOutliers.Count == 0)
            {
                sb.AppendLine("  none");
            }
            else
            {
                foreach (var outlier in priceStability.WorstOutliers.Take(10))
                    sb.AppendLine($"  {outlier.cityId}/{outlier.itemId}: ratio={outlier.ratio:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("--- NPC Snapshot ---");
            sb.AppendLine($"Agents: {npcSnapshot.CountBefore} -> {npcSnapshot.CountAfter}");
            sb.AppendLine($"Total money: {npcSnapshot.TotalMoneyBefore} -> {npcSnapshot.TotalMoneyAfter}");
            sb.AppendLine($"Total inventory units: {npcSnapshot.TotalItemsBefore} -> {npcSnapshot.TotalItemsAfter}");
            sb.AppendLine($"Inventory market value: {npcSnapshot.InventoryMarketValueBefore} -> {npcSnapshot.InventoryMarketValueAfter}");
            sb.AppendLine($"Total debt: {npcSnapshot.TotalDebtBefore:F0} -> {npcSnapshot.TotalDebtAfter:F0}");
            sb.AppendLine($"Net wealth: {npcSnapshot.NetWealthBefore:F0} -> {npcSnapshot.NetWealthAfter:F0}");
            sb.AppendLine($"Net wealth per active NPC: {CalculateAverageWealth(npcSnapshot.NetWealthBefore, npcSnapshot.CountBefore):F1} -> {CalculateAverageWealth(npcSnapshot.NetWealthAfter, npcSnapshot.CountAfter):F1}");

            sb.AppendLine();
            sb.AppendLine("--- Trade Activity ---");
            sb.AppendLine($"Supplies bought units: {npcSnapshot.SuppliesBoughtUnits}");
            sb.AppendLine($"Goods bought units: {npcSnapshot.GoodsBoughtUnits}");
            sb.AppendLine($"Goods sold units: {npcSnapshot.GoodsSoldUnits}");
            sb.AppendLine($"Money spent on supplies: {npcSnapshot.MoneySpentOnSupplies}");
            sb.AppendLine($"Money spent on goods: {npcSnapshot.MoneySpentOnGoods}");
            sb.AppendLine($"Money earned from goods: {npcSnapshot.MoneyEarnedFromGoods}");

            NpcMobilitySummary mobility = CalculateNpcMobility(npcStates);
            if (mobility.SampledAgents > 0)
            {
                sb.AppendLine();
                sb.AppendLine("--- NPC Mobility ---");
                sb.AppendLine($"NPCs with no city changes: {mobility.NoCityChanges}/{mobility.SampledAgents} ({mobility.NoCityChangesRate:P0})");
                sb.AppendLine($"NPCs stuck on one route pair: {mobility.SingleRoutePair}/{mobility.SampledAgents} ({mobility.SingleRoutePairRate:P0})");
                sb.AppendLine($"Average sampled city changes per NPC: {mobility.AverageCityChanges:F1}");
            }

            sb.AppendLine();
            sb.AppendLine("--- Best items per route ---");
            foreach (var (from, to) in routes)
            {
                var routeItems = routeProfitability
                    .Where(r => r.FromCity == from && r.ToCity == to && r.ProfitPct > 0 && SimulationRouteAnalyzer.IsRouteAnalysisItem(r.ItemId))
                    .GroupBy(r => r.ItemId)
                    .Select(g => new { ItemId = g.Key, AvgProfit = g.Average(r => r.ProfitPct) })
                    .OrderByDescending(x => x.AvgProfit)
                    .Take(3)
                    .ToList();

                sb.Append($"  {from} -> {to}: ");
                if (routeItems.Count == 0)
                    sb.AppendLine("no profitable items");
                else
                    sb.AppendLine(string.Join(", ", routeItems.Select(x => $"{x.ItemId} +{x.AvgProfit:F0}%")));
            }

            sb.AppendLine();
            sb.AppendLine("--- City buy/sell spread ---");
            var cityGroups = priceHistory
                .Where(e => e.BuyPrice > 0 && e.SellPrice > 0)
                .GroupBy(e => e.CityId);
            foreach (var cityGroup in cityGroups)
            {
                float avgSpread = (float)cityGroup.Average(e => (e.SellPrice - e.BuyPrice) / (float)e.BuyPrice * 100f);
                sb.AppendLine($"  {cityGroup.Key}: avg spread {avgSpread:F1}%");
            }

            return sb.ToString().TrimEnd();
        }

        private static float CalculateAverageWealth(float totalWealth, int agentCount)
        {
            return agentCount > 0 ? totalWealth / agentCount : 0f;
        }
    }
}
