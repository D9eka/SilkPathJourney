using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Npc.Editor;
using Internal.Scripts.Npc.Lifecycle;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessSimulationWindow : EditorWindow
    {
        [SerializeField] private RoadGraphSnapshot _snapshot;
        [SerializeField] private NpcSimulationSettings _npcSettings;
        [SerializeField] private EconomyDatabase _economyDb;
        [SerializeField] private GameBalanceConfig _balanceConfig;
        [SerializeField] private EconomySimulationSettings _econSimSettings;
        [SerializeField] private TimeSpeedConfig _timeConfig;
        [SerializeField] private CaravanDatabase _caravanDb;
        [SerializeField] private GuildSettings _guildSettings;
        [SerializeField] private CultureAdjacencyData _cultureAdjacency;

        [SerializeField] private int _days = 1000;
        [SerializeField] private int _sampleInterval = 10;
        [SerializeField] private int _seed = 42;

        private SimulationMetrics? _lastMetrics;
        private Vector2 _summaryScroll;
        private string _statusMessage = "";
        private float _elapsedSeconds;

        [MenuItem("SPJ/Debug/Headless NPC Simulation")]
        public static void ShowWindow()
        {
            GetWindow<HeadlessSimulationWindow>("Headless NPC Simulation");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("=== Headless NPC Simulation ===", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawAssetReferences();
            EditorGUILayout.Space(4);

            DrawParameters();
            EditorGUILayout.Space(4);

            DrawButtons();
            EditorGUILayout.Space(4);

            DrawStatus();

            if (_lastMetrics.HasValue)
            {
                EditorGUILayout.Space(4);
                DrawMetrics(_lastMetrics.Value);
            }
        }

        private void DrawAssetReferences()
        {
            EditorGUILayout.LabelField("Asset References", EditorStyles.boldLabel);

            _snapshot = (RoadGraphSnapshot)EditorGUILayout.ObjectField(
                "Snapshot", _snapshot, typeof(RoadGraphSnapshot), false);
            _npcSettings = (NpcSimulationSettings)EditorGUILayout.ObjectField(
                "NPC Settings", _npcSettings, typeof(NpcSimulationSettings), false);
            _economyDb = (EconomyDatabase)EditorGUILayout.ObjectField(
                "Economy DB", _economyDb, typeof(EconomyDatabase), false);
            _balanceConfig = (GameBalanceConfig)EditorGUILayout.ObjectField(
                "Balance Config", _balanceConfig, typeof(GameBalanceConfig), false);
            _econSimSettings = (EconomySimulationSettings)EditorGUILayout.ObjectField(
                "Econ Sim Settings", _econSimSettings, typeof(EconomySimulationSettings), false);
            _timeConfig = (TimeSpeedConfig)EditorGUILayout.ObjectField(
                "Time Config", _timeConfig, typeof(TimeSpeedConfig), false);
            _caravanDb = (CaravanDatabase)EditorGUILayout.ObjectField(
                "Caravan DB", _caravanDb, typeof(CaravanDatabase), false);
            _guildSettings = (GuildSettings)EditorGUILayout.ObjectField(
                "Guild Settings", _guildSettings, typeof(GuildSettings), false);
            _cultureAdjacency = (CultureAdjacencyData)EditorGUILayout.ObjectField(
                "Culture Adjacency", _cultureAdjacency, typeof(CultureAdjacencyData), false);
        }

        private void DrawParameters()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            _days = EditorGUILayout.IntField("Days", _days);
            _sampleInterval = EditorGUILayout.IntField("Sample Interval", _sampleInterval);
            _seed = EditorGUILayout.IntField("Seed", _seed);
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Run Simulation"))
                RunSimulation();

            if (GUILayout.Button("Auto-Load Assets"))
                AutoLoadAssets();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            if (!string.IsNullOrEmpty(_statusMessage))
                EditorGUILayout.LabelField($"Status: {_statusMessage}");
        }

        private void DrawMetrics(SimulationMetrics m)
        {
            EditorGUILayout.LabelField("Metrics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Prices Stable: {CheckMark(m.PricesStable)}");
            EditorGUILayout.LabelField($"Profit in Range (15-25%): {CheckMark(m.ProfitInRange)}");
            EditorGUILayout.LabelField($"Routes Change: {CheckMark(m.RoutesChange)}");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
            _summaryScroll = EditorGUILayout.BeginScrollView(_summaryScroll, GUILayout.Height(220));
            EditorGUILayout.TextArea(m.Summary);
            EditorGUILayout.EndScrollView();
        }

        private void AutoLoadAssets()
        {
            _snapshot = AssetDatabase.LoadAssetAtPath<RoadGraphSnapshot>(
                "Assets/Internal/Data/RoadGraphSnapshot.asset");
            _npcSettings = AssetDatabase.LoadAssetAtPath<NpcSimulationSettings>(
                "Assets/Internal/Data/NpcSimulationSettings.asset");
            _economyDb = AssetDatabase.LoadAssetAtPath<EconomyDatabase>(
                "Assets/Internal/Data/Generated/Databases/EconomyDatabase.asset");
            _balanceConfig = AssetDatabase.LoadAssetAtPath<GameBalanceConfig>(
                "Assets/Internal/Data/GameBalanceConfig.asset");
            _econSimSettings = AssetDatabase.LoadAssetAtPath<EconomySimulationSettings>(
                "Assets/Internal/Data/EconomySimulationSettings.asset");
            _timeConfig = AssetDatabase.LoadAssetAtPath<TimeSpeedConfig>(
                "Assets/Internal/Data/TimeSpeedConfig.asset");
            _caravanDb = AssetDatabase.LoadAssetAtPath<CaravanDatabase>(
                "Assets/Internal/Data/Generated/Databases/CaravanDatabase.asset");
            _guildSettings = AssetDatabase.LoadAssetAtPath<GuildSettings>(
                "Assets/Internal/Data/GuildSettings.asset");
            _cultureAdjacency = AssetDatabase.LoadAssetAtPath<CultureAdjacencyData>(
                "Assets/Internal/Data/Generated/Databases/CultureAdjacencyData.asset");

            int loaded = CountLoaded();
            _statusMessage = $"Auto-loaded {loaded}/9 assets.";
            Repaint();
        }

        private int CountLoaded()
        {
            int n = 0;
            if (_snapshot != null) n++;
            if (_npcSettings != null) n++;
            if (_economyDb != null) n++;
            if (_balanceConfig != null) n++;
            if (_econSimSettings != null) n++;
            if (_timeConfig != null) n++;
            if (_caravanDb != null) n++;
            if (_guildSettings != null) n++;
            if (_cultureAdjacency != null) n++;
            return n;
        }

        private void RunSimulation()
        {
            if (!ValidateAssets())
                return;

            var sw = Stopwatch.StartNew();
            _lastMetrics = null;
            _statusMessage = "Running...";

            var runner = new NpcEconomySimulationRunner();
            var priceHistory = new List<DayPriceEntry>();
            var routeProfitability = new List<RouteProfitEntry>();
            var cityStocks = new List<CityStockEntry>();
            var npcStates = new List<NpcStateEntry>();
            var priceExportBuffer = new List<DayPriceEntry>();
            var routeExportBuffer = new List<RouteProfitEntry>();
            var stockExportBuffer = new List<CityStockEntry>();
            var npcStateExportBuffer = new List<NpcStateEntry>();

            string[] trackedItems = _economyDb.Items.Count > 0
                ? _economyDb.Items.ConvertAll(i => i.Id).ToArray()
                : Array.Empty<string>();

            HeadlessSimulationContainer container = null;

            try
            {
                container = HeadlessSimulationContainer.Build(
                    _snapshot, _npcSettings, _economyDb, _balanceConfig,
                    _econSimSettings, _timeConfig, _caravanDb, _guildSettings, _cultureAdjacency, _seed);

                var routes = SimulationRouteAnalyzer.BuildRoutes(container.EconomyDb);

                int npcCountBefore = container.Simulator.Agents.Count;
                int moneyBefore = container.Simulator.Agents.Sum(a => a.EconomyState.Money);
                int itemsBefore = container.Simulator.Agents.Sum(a => SimulationRouteAnalyzer.CountInventoryUnits(a.EconomyState.Inventory));
                int inventoryValueBefore = container.Simulator.Agents.Sum(a =>
                    SimulationRouteAnalyzer.EstimateInventoryMarketValue(
                        a.EconomyState.Inventory,
                        a.CurrentNodeId,
                        a.DestinationNodeId,
                        container.EconomyDb,
                        container.PriceService));
                float debtBefore = container.Simulator.Agents.Sum(a => a.EconomyState.Debt);

                string exportDir = Path.Combine(Application.dataPath, "..", "SimulationExports");
                Directory.CreateDirectory(exportDir);
                string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                bool headerWritten = false;

                for (int d = 0; d < _days; d++)
                {
                    container.DayTracker.AdvanceDays(1);
                    int currentDay = container.DayTracker.CurrentDay;
                    container.Simulator.AdvanceDay(currentDay);

                    if (d % _sampleInterval == 0)
                    {
                        HeadlessSimulationDataCollector.SamplePricesAndRoutes(
                            container, currentDay, trackedItems, routes,
                            priceHistory, routeProfitability,
                            priceExportBuffer, routeExportBuffer);
                        HeadlessSimulationDataCollector.SampleStocks(container, currentDay, cityStocks, stockExportBuffer);
                        HeadlessSimulationDataCollector.SampleNpcStates(container, currentDay, npcStates, npcStateExportBuffer);
                    }

                    if (d % 100 == 0)
                    {
                        EditorUtility.DisplayProgressBar("Headless Simulation", $"Day {d}/{_days}", (float)d / _days);

                        if ((priceExportBuffer.Count + routeExportBuffer.Count + stockExportBuffer.Count + npcStateExportBuffer.Count) > SimulationCsvExporter.FlushBufferThreshold)
                        {
                            HeadlessSimulationDataCollector.FlushCsv(exportDir, ts, headerWritten, priceExportBuffer, routeExportBuffer, stockExportBuffer, npcStateExportBuffer);
                            headerWritten = true;
                            priceExportBuffer.Clear();
                            routeExportBuffer.Clear();
                            stockExportBuffer.Clear();
                            npcStateExportBuffer.Clear();
                        }
                    }
                }

                int npcCountAfter = container.Simulator.Agents.Count;
                int moneyAfter = container.Simulator.Agents.Sum(a => a.EconomyState.Money);
                int itemsAfter = container.Simulator.Agents.Sum(a => SimulationRouteAnalyzer.CountInventoryUnits(a.EconomyState.Inventory));
                int inventoryValueAfter = container.Simulator.Agents.Sum(a =>
                    SimulationRouteAnalyzer.EstimateInventoryMarketValue(
                        a.EconomyState.Inventory,
                        a.CurrentNodeId,
                        a.DestinationNodeId,
                        container.EconomyDb,
                        container.PriceService));
                float debtAfter = container.Simulator.Agents.Sum(a => a.EconomyState.Debt);

                if (priceExportBuffer.Count > 0 || routeExportBuffer.Count > 0 || stockExportBuffer.Count > 0 || npcStateExportBuffer.Count > 0)
                    HeadlessSimulationDataCollector.FlushCsv(exportDir, ts, headerWritten, priceExportBuffer, routeExportBuffer, stockExportBuffer, npcStateExportBuffer);

                runner.FeedData(priceHistory, routeProfitability, cityStocks, npcStates,
                    new NpcSnapshot
                    {
                        CountBefore = npcCountBefore,
                        TotalMoneyBefore = moneyBefore,
                        TotalItemsBefore = itemsBefore,
                        CountAfter = npcCountAfter,
                        TotalMoneyAfter = moneyAfter,
                        TotalItemsAfter = itemsAfter,
                        SuppliesBoughtUnits = container.Simulator.TotalSuppliesBoughtUnits,
                        GoodsBoughtUnits = container.Simulator.TotalGoodsBoughtUnits,
                        GoodsSoldUnits = container.Simulator.TotalGoodsSoldUnits,
                        MoneySpentOnSupplies = container.Simulator.TotalMoneySpentOnSupplies,
                        MoneySpentOnGoods = container.Simulator.TotalMoneySpentOnGoods,
                        MoneyEarnedFromGoods = container.Simulator.TotalMoneyEarnedFromGoods,
                        InventoryMarketValueBefore = inventoryValueBefore,
                        InventoryMarketValueAfter = inventoryValueAfter,
                        TotalDebtBefore = debtBefore,
                        TotalDebtAfter = debtAfter,
                        NetWealthBefore = moneyBefore + inventoryValueBefore - debtBefore,
                        NetWealthAfter = moneyAfter + inventoryValueAfter - debtAfter
                    });

                _lastMetrics = runner.CalculateMetrics();

                sw.Stop();
                _elapsedSeconds = (float)sw.Elapsed.TotalSeconds;
                _statusMessage = $"Completed in {_elapsedSeconds:F1}s. Exported to SimulationExports/";
                Debug.Log($"[HeadlessSimulationWindow] Simulation done in {_elapsedSeconds:F1}s.");
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error: {ex.Message}";
                Debug.LogException(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (container?.Simulator != null)
                {
                    var s = container.Simulator;
                    Debug.Log(
                        $"[HeadlessSim] Arrivals={s.TotalArrivals}, Trades={s.TotalTrades}, " +
                        $"SuppliesBought={s.TotalSuppliesBoughtUnits}, GoodsBought={s.TotalGoodsBoughtUnits}, GoodsSold={s.TotalGoodsSoldUnits}, " +
                        $"MoneySpentOnSupplies={s.TotalMoneySpentOnSupplies}, MoneySpentOnGoods={s.TotalMoneySpentOnGoods}, " +
                        $"MoneyEarnedFromGoods={s.TotalMoneyEarnedFromGoods}, " +
                        $"ContractsCompleted={s.TotalContractsCompleted}, ContractRewards={s.TotalContractRewards}");
                }
                container?.Dispose();
            }

            Repaint();
        }

        private bool ValidateAssets()
        {
            if (_snapshot == null || _npcSettings == null || _economyDb == null ||
                _balanceConfig == null || _econSimSettings == null ||
                _timeConfig == null || _caravanDb == null || _guildSettings == null)
            {
                _statusMessage = "Error: not all asset references are set. Use Auto-Load Assets.";
                Repaint();
                return false;
            }
            return true;
        }

        private static string CheckMark(bool value) => value ? "OK" : "FAIL";
    }
}
