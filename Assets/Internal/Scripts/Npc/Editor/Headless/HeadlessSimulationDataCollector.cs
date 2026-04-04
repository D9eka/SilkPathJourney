using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Npc.Editor;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public static class HeadlessSimulationDataCollector
    {
        public static void SamplePricesAndRoutes(
            HeadlessSimulationContainer container, int currentDay, string[] trackedItems,
            (string from, string to)[] routes,
            List<DayPriceEntry> priceHistory,
            List<RouteProfitEntry> routeProfitability,
            List<DayPriceEntry> priceExportBuffer,
            List<RouteProfitEntry> routeExportBuffer)
        {
            foreach (var city in container.EconomyDb.Cities)
            {
                foreach (string itemId in trackedItems)
                {
                    int buy = container.PriceService.GetPrice(city.Id, itemId, TradePriceKind.BuyFromCity, false);
                    int sell = container.PriceService.GetPrice(city.Id, itemId, TradePriceKind.SellToCity, false);
                    var entry = new DayPriceEntry
                    {
                        Day = currentDay, CityId = city.Id, ItemId = itemId,
                        BuyPrice = buy, SellPrice = sell
                    };
                    priceHistory.Add(entry);
                    priceExportBuffer.Add(entry);
                }
            }

            foreach (var (from, to) in routes)
            {
                foreach (string itemId in trackedItems)
                {
                    int buy = container.PriceService.GetPrice(from, itemId, TradePriceKind.BuyFromCity, false);
                    int sell = container.PriceService.GetPrice(to, itemId, TradePriceKind.SellToCity, false);
                    float profit = buy > 0 ? (sell - buy) / (float)buy * 100f : 0f;
                    var entry = new RouteProfitEntry
                    {
                        Day = currentDay, FromCity = from, ToCity = to, ItemId = itemId, ProfitPct = profit
                    };
                    routeProfitability.Add(entry);
                    routeExportBuffer.Add(entry);
                }
            }
        }

        public static void SampleStocks(
            HeadlessSimulationContainer container, int currentDay,
            List<CityStockEntry> cityStocks,
            List<CityStockEntry> stockExportBuffer)
        {
            foreach (var city in container.EconomyDb.Cities)
            {
                var cityInv = container.InventoryRepo.GetCityInventory(city.Id);
                if (cityInv?.Inventory?.Items == null) continue;
                foreach (var stack in cityInv.Inventory.Items)
                {
                    var entry = new CityStockEntry
                    {
                        Day = currentDay, CityId = city.Id, ItemId = stack.ItemId, Stock = stack.Count
                    };
                    cityStocks.Add(entry);
                    stockExportBuffer.Add(entry);
                }
            }
        }

        public static void SampleNpcStates(
            HeadlessSimulationContainer container, int currentDay,
            List<NpcStateEntry> npcStates,
            List<NpcStateEntry> npcStateExportBuffer)
        {
            foreach (var agent in container.Simulator.Agents)
            {
                var entry = new NpcStateEntry
                {
                    Day = currentDay,
                    NpcName = agent.EconomyState.Name,
                    Archetype = agent.EconomyState.Archetype.ToString(),
                    Experience = agent.EconomyState.Experience.ToString(),
                    Money = agent.EconomyState.Money,
                    Debt = agent.EconomyState.Debt,
                    ItemCount = SimulationRouteAnalyzer.CountInventoryUnits(agent.EconomyState.Inventory),
                    CurrentCity = agent.CurrentNodeId ?? "",
                    Destination = agent.DestinationNodeId ?? ""
                };
                npcStates.Add(entry);
                npcStateExportBuffer.Add(entry);
            }
        }

        public static void FlushCsv(
            string dir, string ts, bool append,
            List<DayPriceEntry> prices,
            List<RouteProfitEntry> routes,
            List<CityStockEntry> stocks,
            List<NpcStateEntry> npcs)
        {
            SimulationCsvExporter.AppendCsvLines(Path.Combine(dir, $"headless_prices_{ts}.csv"), append,
                "day,city_id,item_id,buy_price,sell_price",
                prices.Select(e => $"{e.Day},{e.CityId},{e.ItemId},{e.BuyPrice},{e.SellPrice}"));

            SimulationCsvExporter.AppendCsvLines(Path.Combine(dir, $"headless_routes_{ts}.csv"), append,
                "day,from_city,to_city,item_id,profit_pct",
                routes.Select(e => $"{e.Day},{e.FromCity},{e.ToCity},{e.ItemId},{e.ProfitPct.ToString("F1", CultureInfo.InvariantCulture)}"));

            SimulationCsvExporter.AppendCsvLines(Path.Combine(dir, $"headless_stocks_{ts}.csv"), append,
                "day,city_id,item_id,stock",
                stocks.Select(e => $"{e.Day},{e.CityId},{e.ItemId},{e.Stock}"));

            SimulationCsvExporter.AppendCsvLines(Path.Combine(dir, $"headless_npc_state_{ts}.csv"), append,
                "day,npc_name,archetype,experience,money,debt,item_count,current_city,destination",
                npcs.Select(e => $"{e.Day},{e.NpcName},{e.Archetype},{e.Experience},{e.Money},{e.Debt.ToString("F0", CultureInfo.InvariantCulture)},{e.ItemCount},{e.CurrentCity},{e.Destination}"));
        }

    }
}
