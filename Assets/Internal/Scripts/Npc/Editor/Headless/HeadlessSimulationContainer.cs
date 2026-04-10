using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Tariff;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Behavior;
using Internal.Scripts.Npc.Behavior.Actions;
using Internal.Scripts.Npc.Behavior.Phases;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using Internal.Scripts.World.State;
using Internal.Scripts.WorldModifiers;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessSimulationContainer : IDisposable
    {
        public HeadlessNpcLifeSimulator Simulator { get; }
        public CityEconomySimulator EconomySimulator { get; }
        public CityTradePriceService PriceService { get; }
        public EconomyDatabase EconomyDb { get; }
        public InventoryRepository InventoryRepo { get; }
        public SaveRepository SaveRepo { get; }
        public DayTracker DayTracker { get; }

        private readonly List<IDisposable> _disposables = new();

        private HeadlessSimulationContainer(
            HeadlessNpcLifeSimulator simulator,
            CityEconomySimulator economySimulator,
            CityTradePriceService priceService,
            EconomyDatabase economyDb,
            InventoryRepository inventoryRepo,
            SaveRepository saveRepo,
            DayTracker dayTracker)
        {
            Simulator = simulator;
            EconomySimulator = economySimulator;
            PriceService = priceService;
            EconomyDb = economyDb;
            InventoryRepo = inventoryRepo;
            SaveRepo = saveRepo;
            DayTracker = dayTracker;
        }

        public static HeadlessSimulationContainer Build(
            RoadGraphSnapshot snapshot,
            NpcSimulationSettings npcSettings,
            EconomyDatabase economyDb,
            GameBalanceConfig balanceConfig,
            EconomySimulationSettings econSimSettings,
            TimeSpeedConfig timeConfig,
            CaravanDatabase caravanDb,
            GuildSettings guildSettings,
            CultureAdjacencyData cultureAdjacency,
            int seed)
        {
            var disposables = new List<IDisposable>();

            var worldState = new AlwaysActiveWorldState();

            var activeSlot = new ActiveSaveSlot();
            activeSlot.CreateNew();

            var inMemSave = new InMemorySaveService();

            var saveRepo = new SaveRepository(inMemSave, activeSlot, balanceConfig);

            var gameClock = new GameClock(timeConfig);

            var dayTracker = new DayTracker(saveRepo, balanceConfig, gameClock, worldState);

            var itemCatalog = new ItemCatalog(economyDb);

            var weightCalc = new ItemWeightCalculator(itemCatalog);

            var headlessPlayerConfig = ScriptableObject.CreateInstance<PlayerConfig>();
            var economySaveBuilder = new EconomySaveBuilder(economyDb, headlessPlayerConfig, caravanDb, econSimSettings, guildSettings);
            saveRepo.Data.Economy = economySaveBuilder.Build();

            var inventoryRepo = new InventoryRepository(saveRepo);
            inventoryRepo.Initialize();

            var playerResRepo = new PlayerResourceRepository(saveRepo);
            playerResRepo.Initialize();
            disposables.Add(playerResRepo);

            var skillRepo = new PlayerSkillRepository(saveRepo, balanceConfig);
            skillRepo.Initialize();
            disposables.Add(skillRepo);

            var langRepo = new PlayerLanguageRepository(saveRepo);
            langRepo.Initialize();
            disposables.Add(langRepo);

            var worldModRepo = new WorldModifierRepository(saveRepo);

            var modQuery = new ModifierEffectQuery(worldModRepo, economyDb);

            var skillMod = new TradePriceSkillModifier(skillRepo, balanceConfig);

            var langMod = new LanguagePriceModifier(langRepo, playerResRepo, caravanDb, economyDb);

            var modifiers = new TradePriceModifiers(skillMod, langMod, modQuery, economyDb);

            var profileService = new CityMarketProfileService(economyDb, itemCatalog);

            var cultureDistance = new CultureDistanceService(cultureAdjacency);
            var priceService = new CityTradePriceService(profileService, econSimSettings, inventoryRepo, itemCatalog, modifiers, economyDb, cultureDistance, balanceConfig, guildSettings);

            var economySimulator = new CityEconomySimulator(dayTracker, inventoryRepo, profileService, economyDb, itemCatalog, guildSettings);
            economySimulator.Initialize();
            disposables.Add(economySimulator);

            var headlessNodeLookup = new HeadlessRoadNodeLookup(snapshot);
            var headlessNetwork = new HeadlessRoadNetwork(snapshot);
            var headlessCityResolver = new HeadlessCityNodeResolver(snapshot, economyDb);
            var headlessPathFinder = new HeadlessPathFinder(snapshot);

            var radiusResolver = new CityRadiusService(headlessNetwork, headlessCityResolver);

            var transService = new CityTransactionService(inventoryRepo, priceService, economyDb, itemCatalog, guildSettings);

            var supplyPlanner = new NpcSupplyPlanner(transService, npcSettings, headlessPathFinder, inventoryRepo, priceService);

            var sellEstimator = new NpcSellEstimator(priceService, itemCatalog, economyDb, cultureDistance, balanceConfig);

            var knowledgeService = new NpcKnowledgeService(worldModRepo, radiusResolver, headlessCityResolver, dayTracker, npcSettings);

            var guildService = new GuildService(saveRepo, playerResRepo, inventoryRepo, economyDb, dayTracker, headlessPathFinder, guildSettings, itemCatalog);
            var tariffService = new TariffService(economyDb, playerResRepo, itemCatalog, inventoryRepo, guildService, guildSettings, balanceConfig);

            var sellService = new NpcSellService(transService, npcSettings, itemCatalog, economyDb, dayTracker);
            var guildTradeService = new NpcGuildTradeService(economyDb, inventoryRepo, itemCatalog, npcSettings, guildSettings, supplyPlanner, dayTracker);
            var trader = new NpcTrader(transService, supplyPlanner, inventoryRepo, itemCatalog, weightCalc, npcSettings, sellEstimator, economyDb, dayTracker, headlessCityResolver, cultureDistance, sellService, balanceConfig, guildTradeService);
            var routeDecisionService = new NpcRouteDecisionService(npcSettings, supplyPlanner, knowledgeService, trader);

            var learnAction = new LearnKnowledgeAction(knowledgeService);
            var tariffAction = new ChargeTariffAction(tariffService);
            var debtRepaymentAction = new DebtRepaymentAction(guildTradeService);
            var sellAction = new SellGoodsAction(sellService);
            var completeContractAction = new CompleteContractAction(inventoryRepo);
            var takeContractAction = new TakeContractAction(guildTradeService);
            var chooseRouteAction = new ChooseRouteAction(routeDecisionService);
            var buyAction = new BuyGoodsAction(trader);
            var guildCreditAction = new GuildCreditAction(guildTradeService);

            var visitActionFactory = new NpcVisitActionFactory(
                learnAction, tariffAction, debtRepaymentAction, sellAction, completeContractAction,
                takeContractAction, chooseRouteAction, buyAction, guildCreditAction);

            var contractExpirationPhase = new ContractExpirationPhase(inventoryRepo);
            var foragePhase = new ForagePhase(npcSettings);
            var consumptionPhase = new ConsumptionPhase(npcSettings);
            var starvationPhase = new StarvationPhase(npcSettings);

            var dayPhaseFactory = new NpcDayPhaseFactory(
                contractExpirationPhase, foragePhase, consumptionPhase, starvationPhase);

            var profileRegistry = new NpcBehaviorProfileRegistry(npcSettings.DefaultBehaviorProfile, npcSettings.BehaviorProfiles);

            var visitProcessor = new NpcCityVisitProcessor(profileRegistry, visitActionFactory);
            var dayProcessor = new NpcDayProcessor(profileRegistry, dayPhaseFactory);

            var simulator = new HeadlessNpcLifeSimulator(
                npcSettings,
                snapshot,
                headlessCityResolver,
                routeDecisionService,
                visitProcessor,
                dayProcessor,
                seed);
            simulator.Initialize(headlessNetwork);

            var container = new HeadlessSimulationContainer(
                simulator, economySimulator, priceService, economyDb, inventoryRepo, saveRepo, dayTracker);

            container._disposables.AddRange(disposables);

            return container;
        }

        public void Dispose()
        {
            foreach (IDisposable d in _disposables)
                d.Dispose();
            _disposables.Clear();
        }
    }
}
