using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Save;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor
{
    public sealed class NpcEconomySimulationRunner
    {
        private readonly List<DayPriceEntry> _priceHistory = new();
        private readonly List<RouteProfitEntry> _routeProfitability = new();
        private readonly List<CityStockEntry> _cityStocks = new();
        private readonly List<NpcStateEntry> _npcStates = new();
        private bool _isRunning;
        private NpcSnapshot _npcSnapshot;
        private (string from, string to)[] _routes = System.Array.Empty<(string, string)>();

        public IReadOnlyList<DayPriceEntry> PriceHistory => _priceHistory;
        public IReadOnlyList<RouteProfitEntry> RouteProfitability => _routeProfitability;
        public bool IsRunning => _isRunning;

        public void FeedData(
            IEnumerable<DayPriceEntry> prices,
            IEnumerable<RouteProfitEntry> routes,
            IEnumerable<CityStockEntry> stocks,
            IEnumerable<NpcStateEntry> npcStates,
            NpcSnapshot snapshot)
        {
            _priceHistory.Clear();
            _priceHistory.AddRange(prices);
            _routeProfitability.Clear();
            _routeProfitability.AddRange(routes);
            _cityStocks.Clear();
            _cityStocks.AddRange(stocks);
            _npcStates.Clear();
            _npcStates.AddRange(npcStates);
            _npcSnapshot = snapshot;
        }

        public void Run(int days, DayTracker dayTracker, CityTradePriceService priceService,
            EconomyDatabase economyDb, CityEconomySimulator economySimulator = null,
            SaveRepository saveRepository = null, NpcLifeSimulator simulator = null,
            InventoryRepository inventoryRepo = null, bool fullSimulation = false,
            int sampleInterval = 10)
        {
            _priceHistory.Clear();
            _routeProfitability.Clear();
            _cityStocks.Clear();
            _npcStates.Clear();
            _isRunning = true;

            _routes = SimulationRouteAnalyzer.BuildRoutes(economyDb);

            string exportDir = Path.Combine(Application.dataPath, "..", "SimulationExports");
            Directory.CreateDirectory(exportDir);
            string exportTimestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            bool headerWritten = false;
            var priceExportBuffer = new List<DayPriceEntry>();
            var routeExportBuffer = new List<RouteProfitEntry>();
            var stockExportBuffer = new List<CityStockEntry>();
            var npcStateExportBuffer = new List<NpcStateEntry>();

            int npcCountBefore = 0;
            int npcTotalMoneyBefore = 0;
            int npcTotalItemsBefore = 0;
            int npcInventoryValueBefore = 0;
            float npcDebtBefore = 0f;
            if (simulator != null)
            {
                foreach (var agent in simulator.Agents)
                {
                    npcCountBefore++;
                    npcTotalMoneyBefore += agent.EconomyState.Money;
                    npcTotalItemsBefore += SimulationRouteAnalyzer.CountInventoryUnits(agent.EconomyState.Inventory);
                    npcInventoryValueBefore += SimulationRouteAnalyzer.EstimateInventoryMarketValue(
                        agent.EconomyState.Inventory,
                        agent.CurrentNodeId,
                        agent.DestinationNodeId,
                        economyDb,
                        priceService);
                    npcDebtBefore += agent.EconomyState.Debt;
                }
            }

            string[] trackedItems = economyDb.Items.Count > 0
                ? economyDb.Items.ConvertAll(i => i.Id).ToArray()
                : System.Array.Empty<string>();

            try
            {
                for (int d = 0; d < days; d++)
                {
                    if (fullSimulation)
                    {
                        dayTracker.AdvanceDays(1);
                    }
                    else if (economySimulator != null && saveRepository != null)
                    {
                        saveRepository.Data.Player.CurrentDay++;
                        economySimulator.SimulateDay();
                    }
                    else
                    {
                        dayTracker.AdvanceDays(1);
                    }

                    int currentDay = dayTracker.CurrentDay;

                    if (d % sampleInterval == 0)
                    {
                        foreach (var city in economyDb.Cities)
                        {
                            foreach (string itemId in trackedItems)
                            {
                                int buyPrice = priceService.GetPrice(city.Id, itemId, TradePriceKind.BuyFromCity, false);
                                int sellPrice = priceService.GetPrice(city.Id, itemId, TradePriceKind.SellToCity, false);

                                var entry = new DayPriceEntry
                                {
                                    Day = currentDay,
                                    CityId = city.Id,
                                    ItemId = itemId,
                                    BuyPrice = buyPrice,
                                    SellPrice = sellPrice
                                };
                                _priceHistory.Add(entry);
                                priceExportBuffer.Add(entry);
                            }
                        }

                        foreach (var (from, to) in _routes)
                        {
                            foreach (string itemId in trackedItems)
                            {
                                int buyPrice = priceService.GetPrice(from, itemId, TradePriceKind.BuyFromCity, false);
                                int sellPrice = priceService.GetPrice(to, itemId, TradePriceKind.SellToCity, false);

                                float profitPct = buyPrice > 0
                                    ? (sellPrice - buyPrice) / (float)buyPrice * 100f
                                    : 0f;

                                var entry = new RouteProfitEntry
                                {
                                    Day = currentDay,
                                    FromCity = from,
                                    ToCity = to,
                                    ItemId = itemId,
                                    ProfitPct = profitPct
                                };
                                _routeProfitability.Add(entry);
                                routeExportBuffer.Add(entry);
                            }
                        }
                    }

                    if (d % 10 == 0 && inventoryRepo != null)
                    {
                        foreach (var city in economyDb.Cities)
                        {
                            var cityInv = inventoryRepo.GetCityInventory(city.Id);
                            if (cityInv?.Inventory?.Items == null) continue;
                            foreach (var stack in cityInv.Inventory.Items)
                            {
                                var entry = new CityStockEntry
                                {
                                    Day = currentDay, CityId = city.Id,
                                    ItemId = stack.ItemId, Stock = stack.Count
                                };
                                _cityStocks.Add(entry);
                                stockExportBuffer.Add(entry);
                            }
                        }
                    }

                    if (fullSimulation && d % 10 == 0 && simulator != null)
                    {
                        foreach (var agent in simulator.Agents)
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
                            _npcStates.Add(entry);
                            npcStateExportBuffer.Add(entry);
                        }
                    }

                    if (d % 100 == 0)
                        EditorUtility.DisplayProgressBar("Economy Simulation", $"Day {d}/{days}", (float)d / days);

                    if (d % 100 == 0 && (priceExportBuffer.Count + routeExportBuffer.Count + stockExportBuffer.Count + npcStateExportBuffer.Count) > SimulationCsvExporter.FlushBufferThreshold)
                    {
                        SimulationCsvExporter.AppendExportCsv(exportDir, exportTimestamp, headerWritten,
                            priceExportBuffer, routeExportBuffer, stockExportBuffer, npcStateExportBuffer);
                        headerWritten = true;
                        priceExportBuffer.Clear();
                        routeExportBuffer.Clear();
                        stockExportBuffer.Clear();
                        npcStateExportBuffer.Clear();
                    }
                }

                int npcCountAfter = 0;
                int npcTotalMoneyAfter = 0;
                int npcTotalItemsAfter = 0;
                int npcInventoryValueAfter = 0;
                float npcDebtAfter = 0f;
                if (simulator != null)
                {
                    foreach (var agent in simulator.Agents)
                    {
                        npcCountAfter++;
                        npcTotalMoneyAfter += agent.EconomyState.Money;
                        npcTotalItemsAfter += SimulationRouteAnalyzer.CountInventoryUnits(agent.EconomyState.Inventory);
                        npcInventoryValueAfter += SimulationRouteAnalyzer.EstimateInventoryMarketValue(
                            agent.EconomyState.Inventory,
                            agent.CurrentNodeId,
                            agent.DestinationNodeId,
                            economyDb,
                            priceService);
                        npcDebtAfter += agent.EconomyState.Debt;
                    }
                }
                _npcSnapshot = new NpcSnapshot
                {
                    CountBefore = npcCountBefore, CountAfter = npcCountAfter,
                    TotalMoneyBefore = npcTotalMoneyBefore, TotalMoneyAfter = npcTotalMoneyAfter,
                    TotalItemsBefore = npcTotalItemsBefore, TotalItemsAfter = npcTotalItemsAfter,
                    InventoryMarketValueBefore = npcInventoryValueBefore,
                    InventoryMarketValueAfter = npcInventoryValueAfter,
                    TotalDebtBefore = npcDebtBefore,
                    TotalDebtAfter = npcDebtAfter,
                    NetWealthBefore = npcTotalMoneyBefore + npcInventoryValueBefore - npcDebtBefore,
                    NetWealthAfter = npcTotalMoneyAfter + npcInventoryValueAfter - npcDebtAfter
                };
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isRunning = false;
                if (saveRepository != null)
                    saveRepository.Save();
                SimulationCsvExporter.AutoExportCsv(_priceHistory, _routeProfitability, _cityStocks, _npcStates);
            }
        }

        public SimulationMetrics CalculateMetrics()
        {
            return SimulationMetricsCalculator.CalculateMetrics(
                _priceHistory, _routeProfitability, _npcStates, _npcSnapshot, _routes);
        }
    }
}
