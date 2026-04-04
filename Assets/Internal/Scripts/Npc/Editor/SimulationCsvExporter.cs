using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor
{
    public static class SimulationCsvExporter
    {
        public const int FlushBufferThreshold = 500_000;

        public static void AutoExportCsv(
            IReadOnlyList<DayPriceEntry> priceHistory,
            IReadOnlyList<RouteProfitEntry> routeProfitability,
            IReadOnlyList<CityStockEntry> cityStocks,
            IReadOnlyList<NpcStateEntry> npcStates)
        {
            string dir = Path.Combine(Application.dataPath, "..", "SimulationExports");
            Directory.CreateDirectory(dir);
            string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (priceHistory.Count > 0) ExportPriceHistoryCsv(Path.Combine(dir, $"prices_{ts}.csv"), priceHistory);
            if (routeProfitability.Count > 0) ExportRouteProfitabilityCsv(Path.Combine(dir, $"routes_{ts}.csv"), routeProfitability);
            if (cityStocks.Count > 0) ExportCityStocksCsv(Path.Combine(dir, $"stocks_{ts}.csv"), cityStocks);
            if (npcStates.Count > 0) ExportNpcStateCsv(Path.Combine(dir, $"npc_state_{ts}.csv"), npcStates);
            Debug.Log($"[SimulationRunner] Auto-exported to {dir}");
        }

        public static void AppendExportCsv(string dir, string ts, bool append,
            IEnumerable<DayPriceEntry> prices,
            IEnumerable<RouteProfitEntry> routes,
            IEnumerable<CityStockEntry> stocks,
            IEnumerable<NpcStateEntry> npcStates)
        {
            AppendCsvLines(Path.Combine(dir, $"prices_{ts}.csv"), append,
                "day,city_id,item_id,buy_price,sell_price",
                prices.Select(e => $"{e.Day},{e.CityId},{e.ItemId},{e.BuyPrice},{e.SellPrice}"));
            AppendCsvLines(Path.Combine(dir, $"routes_{ts}.csv"), append,
                "day,from_city,to_city,item_id,profit_pct",
                routes.Select(e => $"{e.Day},{e.FromCity},{e.ToCity},{e.ItemId},{e.ProfitPct.ToString("F1", CultureInfo.InvariantCulture)}"));
            AppendCsvLines(Path.Combine(dir, $"stocks_{ts}.csv"), append,
                "day,city_id,item_id,stock",
                stocks.Select(e => $"{e.Day},{e.CityId},{e.ItemId},{e.Stock}"));
            AppendCsvLines(Path.Combine(dir, $"npc_state_{ts}.csv"), append,
                "day,npc_name,archetype,experience,money,debt,item_count,current_city,destination",
                npcStates.Select(e => $"{e.Day},{e.NpcName},{e.Archetype},{e.Experience},{e.Money},{e.Debt.ToString("F0", CultureInfo.InvariantCulture)},{e.ItemCount},{e.CurrentCity},{e.Destination}"));
        }

        public static void ExportPriceHistoryCsv(string path, IReadOnlyList<DayPriceEntry> priceHistory)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("day,city_id,item_id,buy_price,sell_price");
            foreach (var entry in priceHistory)
                writer.WriteLine($"{entry.Day},{entry.CityId},{entry.ItemId},{entry.BuyPrice},{entry.SellPrice}");
        }

        public static void ExportRouteProfitabilityCsv(string path, IReadOnlyList<RouteProfitEntry> routeProfitability)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("day,from_city,to_city,item_id,profit_pct");
            foreach (var entry in routeProfitability)
                writer.WriteLine($"{entry.Day},{entry.FromCity},{entry.ToCity},{entry.ItemId},{entry.ProfitPct.ToString("F1", CultureInfo.InvariantCulture)}");
        }

        public static void ExportCityStocksCsv(string path, IReadOnlyList<CityStockEntry> cityStocks)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("day,city_id,item_id,stock");
            foreach (var e in cityStocks)
                writer.WriteLine($"{e.Day},{e.CityId},{e.ItemId},{e.Stock}");
        }

        public static void ExportNpcStateCsv(string path, IReadOnlyList<NpcStateEntry> npcStates)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("day,npc_name,archetype,experience,money,debt,item_count,current_city,destination");
            foreach (var e in npcStates)
                writer.WriteLine($"{e.Day},{e.NpcName},{e.Archetype},{e.Experience},{e.Money},{e.Debt.ToString("F0", CultureInfo.InvariantCulture)},{e.ItemCount},{e.CurrentCity},{e.Destination}");
        }

        public static void AppendCsvLines(string path, bool append, string header, IEnumerable<string> lines)
        {
            using var writer = new StreamWriter(path, append);
            if (!append) writer.WriteLine(header);
            foreach (var line in lines)
                writer.WriteLine(line);
        }
    }
}
