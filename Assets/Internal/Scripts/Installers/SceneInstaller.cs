using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Follow;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Tilt;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Tariff;
using Internal.Scripts.Economy.Cities.Smuggling;
using Internal.Scripts.Economy.Cities.Rumors;
using Internal.Scripts.Economy.MarketMemory;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Quests;
using Internal.Scripts.Input;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Behavior;
using Internal.Scripts.Npc.Behavior.Actions;
using Internal.Scripts.Npc.Behavior.Phases;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using Internal.Scripts.Road.Positioning;
using Internal.Scripts.Road.State;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using Internal.Scripts.UI;
using Internal.Scripts.UI.Arrow;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Arrow.DirectionCalculation;
using Internal.Scripts.UI.Arrow.JunctionBalancer;
using Internal.Scripts.UI.Arrow.Placement;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.City;
using Internal.Scripts.UI.Factory;
using Internal.Scripts.UI.PathVisualization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Event;
using Internal.Scripts.UI.Screens.Camp;
using Internal.Scripts.UI.Screens.Event.ConditionLines;
using Internal.Scripts.UI.Screens.Hud;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.Camp;
using Internal.Scripts.Meta;
using Internal.Scripts.Travel.Hazards;
using Internal.Scripts.Travel.Pickups;
using Internal.Scripts.Travel.Triggers;
using Internal.Scripts.Travel.Triggers.Actions;
using Internal.Scripts.UI.Screens.Caravansary.Services;
using Internal.Scripts.Economy.Buildings.Healer;
using Internal.Scripts.Economy.Buildings.Archive;
using Internal.Scripts.Economy.Buildings.Temple;
using Internal.Scripts.Economy.Buildings.Barracks;
using Internal.Scripts.Journal;
using Internal.Scripts.UI.Screens.Building;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Tooltip;
using Internal.Scripts.UI.WorldLabel;
using Internal.Scripts.Utils;
using Internal.Scripts.World.State;
using Internal.Scripts.WorldModifiers;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [Header("Camera")]
        [SerializeField] private DetailSceneBounds _strategicBounds;
        [Header("NPC")]
        [SerializeField] private NpcSpawnEntry[] _spawns;
        [Header("Interactables")]
        [SerializeField] private LayerMask _interactableLayerMask;
        [Header("Arrows")]
        [SerializeField] private ArrowView _arrowPrefab;
        [Header("UI")]
        [SerializeField] private UIScreenRoots _uiScreenRoots;
        [SerializeField] private UnityEngine.UI.Button _enterCityButton;
        [Header("Convoy")]
        [Header("Path Visualization")]
        [SerializeField] private Shader _pathShader;

        public override void InstallBindings()
        {
            Container.Bind<GameClock>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameDayDeltaProvider>().AsSingle();
            Container.Bind<IWorldSimulationState>().To<WorldSimulationState>().AsSingle();
            Container.Bind<SceneLoaderService>().AsSingle();

            Container.BindInterfacesTo<InputSceneSetup>().AsSingle()
                .WithArguments(_interactableLayerMask);

            InstallCamera();
            InstallWorld();
            InstallRoad();
            InstallEconomy();
            InstallNpc();
            InstallWorldModifiers();
            InstallWorldCanvas();
            InstallWorldLabels();
            InstallPlayer();
            InstallScreens();
            InstallEvents();
            InstallQuests();
            InstallCamp();
            InstallPickups();
            InstallHazards();

            Container.BindInterfacesAndSelfTo<AutoSaveController>().AsSingle();
            Container.BindInterfacesTo<CameraSaveController>().AsSingle();

            InstallGameEnd();
        }

        private void InstallCamera()
        {
            Container.Bind<CameraBounds>().AsSingle()
                .WithArguments(_strategicBounds.BoundsCollider, _strategicBounds.CenterTransform);

            Container.BindInterfacesTo<CameraZoomer>().AsSingle();
            Container.BindInterfacesTo<CameraTilter>().AsSingle();
            Container.BindInterfacesTo<CameraMover>().AsSingle();
            Container.BindInterfacesTo<CameraAutoFitter>().AsSingle();
            Container.BindInterfacesTo<CameraFollowService>().AsSingle();
            Container.Bind<MainSceneVisibilityController>().AsSingle();
            Container.Bind<DetailSceneLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraSceneLoader>().AsSingle();

            Container.BindInterfacesAndSelfTo<CameraController>().AsSingle();
            Container.Bind<CityViewAnimator>().AsSingle();
            Container.Bind<CitySceneController>().AsSingle();
            Container.BindInterfacesAndSelfTo<CityEntryService>().AsSingle().NonLazy();
        }

        private void InstallWorld()
        {
            Container.BindInterfacesAndSelfTo<WorldStateController>().AsSingle();
        }

        private void InstallRoad()
        {
            RoadRuntime[] roads = FindObjectsByType<RoadRuntime>(FindObjectsSortMode.None);

            Container.Bind<RoadPoseSampler>().AsSingle();
            Container.BindInstance(roads).AsSingle();
            Container.Bind<RoadSamplerCache>().AsSingle();
            Container.Bind<RoadUnlockService>().AsSingle();

            Container.BindInterfacesAndSelfTo<RoadNodeLookup>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<RoadNetwork>().AsSingle().NonLazy();
            Container.Bind<IRoadPathFinder>().To<RoadPathFinder>().AsSingle();
            Container.BindInterfacesTo<RoadSaveController>().AsSingle();
            Container.Bind<CityRadiusService>().AsSingle();
        }

        private void InstallNpc()
        {
            Container.BindInterfacesAndSelfTo<NpcSimulation>().AsSingle();
            Container.Bind<NpcFactory>().AsSingle();

            Container.BindInstance(_spawns ?? Array.Empty<NpcSpawnEntry>())
                .WhenInjectedInto<NpcBootstrapper>();
            Container.BindInterfacesAndSelfTo<NpcBootstrapper>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<NpcLifeSimulator>().AsSingle().NonLazy();
            Container.Bind<CityTransactionService>().AsSingle();
            Container.Bind<NpcSupplyPlanner>().AsSingle();
            Container.Bind<NpcSellEstimator>().AsSingle();
            Container.Bind<NpcKnowledgeService>().AsSingle();
            Container.Bind<NpcSellService>().AsSingle();
            Container.Bind<NpcGuildTradeService>().AsSingle();
            Container.Bind<NpcTrader>().AsSingle();
            Container.Bind<NpcRouteDecisionService>().AsSingle();
            Container.BindInterfacesTo<NpcSaveController>().AsSingle();

            Container.BindInterfacesTo<EncounterSegmentAction>().AsSingle();

            Container.Bind<LearnKnowledgeAction>().AsSingle();
            Container.Bind<ChargeTariffAction>().AsSingle();
            Container.Bind<DebtRepaymentAction>().AsSingle();
            Container.Bind<SellGoodsAction>().AsSingle();
            Container.Bind<CompleteContractAction>().AsSingle();
            Container.Bind<TakeContractAction>().AsSingle();
            Container.Bind<ChooseRouteAction>().AsSingle();
            Container.Bind<BuyGoodsAction>().AsSingle();
            Container.Bind<GuildCreditAction>().AsSingle();

            Container.Bind<ContractExpirationPhase>().AsSingle();
            Container.Bind<ForagePhase>().AsSingle();
            Container.Bind<ConsumptionPhase>().AsSingle();
            Container.Bind<StarvationPhase>().AsSingle();

            Container.Bind<NpcVisitActionFactory>().AsSingle();
            Container.Bind<NpcDayPhaseFactory>().AsSingle();

            Container.Bind<NpcBehaviorProfileRegistry>().FromMethod(ctx =>
            {
                var settings = ctx.Container.Resolve<NpcSimulationSettings>();
                return new NpcBehaviorProfileRegistry(settings.DefaultBehaviorProfile, settings.BehaviorProfiles);
            }).AsSingle();

            Container.Bind<NpcCityVisitProcessor>().AsSingle();
            Container.Bind<NpcDayProcessor>().AsSingle();
        }

        private void InstallPlayer()
        {
            InstallArrows();
            Container.BindInterfacesTo<PlayerChoiceInputView>().AsSingle();
            Container.Bind<PathHintsCreator>().AsSingle();
            Container.Bind<RoadAgentView>()
                .FromMethod(_ => new GameObject("PlayerView").AddComponent<RoadAgentView>())
                .AsSingle();
            Container.BindInterfacesAndSelfTo<SegmentMover>().AsSingle().WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesAndSelfTo<PlayerNextSegmentsProvider>().AsSingle();
            Container.BindInterfacesTo<PlayerStartMovement>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerSaveController>().AsSingle();
            Container.Bind<OverloadCalculator>().AsSingle();
            Container.BindInterfacesAndSelfTo<CaravanSpeedService>().AsSingle();
            Container.Bind<FoodConsumptionCalculator>().AsSingle();
            Container.Bind<DailyTravelCosts>().AsSingle();
            Container.Bind<TravelEstimator>().AsSingle();
            Container.Bind<CompanionService>().AsSingle();
            Container.Bind<CompanionCosts>().AsSingle();
            Container.Bind<CompanionBonuses>().AsSingle();
            Container.Bind<DraftAnimalService>().AsSingle();
            Container.Bind<CaravanUpgradeService>().AsSingle();
            Container.BindInterfacesTo<PlayerInitializer>().AsSingle();
            Container.BindInterfacesTo<ConvoyVisualizer>().AsSingle();
            Container.BindInterfacesTo<CityNodeResolver>().AsSingle();

            if (_enterCityButton != null)
                Container.BindInterfacesTo<PlayerCityButtonsController>().AsSingle()
                    .WithArguments(_enterCityButton);
        }

        private void InstallEconomy()
        {
            Container.Bind<ItemWeightCalculator>().AsSingle();
            Container.Bind<EconomySaveBuilder>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveBootstrapper>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InventoryRepository>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerResourceRepository>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<Player.Skills.PlayerSkillRepository>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<Player.Languages.PlayerLanguageRepository>().AsSingle().NonLazy();

            Container.Bind<Player.Skills.TradePriceSkillModifier>().AsSingle();
            Container.Bind<Player.Languages.LanguagePriceModifier>().AsSingle();
            Container.Bind<TradePriceModifiers>().AsSingle();
            Container.Bind<CityMarketProfileService>().AsSingle();
            Container.Bind<CityTradePriceService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CityEconomySimulator>().AsSingle().NonLazy();

            Container.Bind<GuildService>().AsSingle();
            Container.Bind<CultureDistanceService>().AsSingle();
            Container.Bind<TariffService>().AsSingle();
            Container.Bind<SmugglingModifierCalculator>().AsSingle();
            Container.Bind<SmugglingCheckService>().AsSingle();
            Container.Bind<RumorService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MarketMemoryService>().AsSingle();
        }

        private void InstallScreens()
        {
            if (_uiScreenRoots != null)
                Container.BindInstance(_uiScreenRoots).AsSingle();

            Container.Bind<InventoryModel>().AsSingle();
            Container.Bind<TradeModel>().AsSingle();
            Container.Bind<HudModel>().AsSingle();

            Container.Bind<IScreenViewModelFactory>().To<ScreenViewModelFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScreenStackService>().AsSingle().WithArguments(ScreenId.Hud);
            Container.BindInterfacesTo<ScreenBackNavigator>().AsSingle();
            Container.BindInterfacesTo<Input.GameSpeedController>().AsSingle();
            Container.BindInterfacesTo<TooltipThemeSetup>().AsSingle();

            Container.Bind<ICityOffering>().To<RestService>().AsSingle();
            Container.Bind<ICityOffering>().To<MainCartRepairService>().AsSingle();
            Container.Bind<ICityOffering>().To<ExtraCartsRepairService>().AsSingle();
        }

        private void InstallArrows()
        {
            Container.Bind<IRoadSidePositionCalculator>()
                .To<RoadSidePositionCalculator>().AsSingle();

            Container.Bind<IArrowPositionCalculator>()
                .To<RoadPoseArrowPositionCalculator>().AsSingle();

            Container.Bind<IArrowDirectionCalculator>()
                .To<RoadPoseArrowDirectionCalculator>().AsSingle();

            Container.BindInterfacesTo<ArrowJunctionBalancer>().AsSingle();

            Container.Bind<ArrowFactory>()
                .AsSingle()
                .WithArguments(_arrowPrefab);

            Container.Bind<IArrowPlacementService>()
                .To<ArrowPlacementService>().AsSingle();

            Container.BindInterfacesTo<RoadPoseArrowsController>()
                .AsSingle();
        }

        private void InstallEvents()
        {
            Container.BindInterfacesAndSelfTo<DayTracker>().AsSingle().NonLazy();

            Container.Bind<Events.Conditions.ResourceEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.InventoryEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.CartEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.LocationEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.CompanionEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.MinSkillEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.MinLanguageProficiencyEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.InCampEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.MinCompanionsEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.CityEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.ReputationConditionEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.MoraleConditionEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.QuestConditionEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.QuestStageEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.QuestFlagEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.NoQuestFlagEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.TravelEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.NoItemEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.DangerAboveEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.BackgroundEvaluator>().AsSingle();
            Container.Bind<Events.Conditions.ConditionEvaluator>().AsSingle();

            Container.Bind<Events.Outcomes.ResourceApplier>().AsSingle();
            Container.Bind<Events.Outcomes.ItemApplier>().AsSingle();
            Container.Bind<Events.Outcomes.CartDurabilityApplier>().AsSingle();
            Container.Bind<Events.Outcomes.CompanionApplier>().AsSingle();
            Container.Bind<Events.Outcomes.SkillXpApplier>().AsSingle();
            Container.Bind<Events.Outcomes.RoadUnlockApplier>().AsSingle();
            Container.Bind<Events.Outcomes.LanguageProficiencyApplier>().AsSingle();
            Container.Bind<Events.Outcomes.SkipDaysApplier>().AsSingle();
            Container.Bind<Events.Outcomes.MoraleApplier>().AsSingle();
            Container.Bind<Events.Outcomes.ReputationApplier>().AsSingle();
            Container.Bind<Events.Outcomes.ExtraCartDurabilityApplier>().AsSingle();
            Container.Bind<Events.Outcomes.MainCartDurabilityApplier>().AsSingle();
            Container.Bind<Events.Outcomes.RemoveItemApplier>().AsSingle();
            Container.Bind<Events.Outcomes.BlockMovementApplier>().AsSingle();
            Container.Bind<Events.Outcomes.CompanionInjuredApplier>().AsSingle();
            Container.Bind<Events.Outcomes.StartQuestApplier>().AsSingle();
            Container.Bind<Events.Outcomes.AdvanceQuestApplier>().AsSingle();
            Container.Bind<Events.Outcomes.CompleteQuestApplier>().AsSingle();
            Container.Bind<Events.Outcomes.FailQuestApplier>().AsSingle();
            Container.Bind<Events.Outcomes.SetQuestFlagApplier>().AsSingle();
            Container.Bind<Events.Outcomes.MarkPendingEndingApplier>().AsSingle();
            Container.Bind<Events.Outcomes.QueueEventApplier>().AsSingle();
            Container.Bind<Events.Outcomes.OutcomeApplier>().AsSingle();

            Container.Bind<SkillCheckService>().AsSingle();
            Container.Bind<EventSelector>().AsSingle();
            Container.Bind<EventOutcomeFormatter>().AsSingle();
            Container.Bind<SkillCheckConditionLine>().AsSingle();
            Container.Bind<ItemConditionLine>().AsSingle();
            Container.Bind<LanguageConditionLine>().AsSingle();
            Container.Bind<CompanionConditionLine>().AsSingle();
            Container.Bind<ConditionLineBuilder>().AsSingle();

            Container.Bind<EventQueueService>().AsSingle();
            Container.Bind<EventToastController>().AsSingle();
            Container.Bind<EventCloseSignal>().AsSingle();
            Container.Bind<RecentEventHistory>().AsSingle();
            Container.BindInterfacesAndSelfTo<CrisisTrigger>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EventTrigger>().AsSingle().NonLazy();

            Container.BindInterfacesTo<MajorEventDailyAction>().AsSingle();
            Container.BindInterfacesTo<MinorEventDailyAction>().AsSingle();
            Container.BindInterfacesTo<DailyTriggerService>().AsSingle();
        }

        private void InstallPickups()
        {
            Container.BindInterfacesAndSelfTo<PickupCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<PickupSpawner>().AsSingle();
            Container.BindInterfacesTo<PickupSegmentAction>().AsSingle();
        }

        private void InstallHazards()
        {
            Container.Bind<HazardSelector>().AsSingle();
            Container.Bind<HazardController>().AsSingle();
            Container.BindInterfacesTo<HazardSegmentAction>().AsSingle();
            Container.Bind<DistanceTrackingService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SegmentTriggerService>().AsSingle().NonLazy();
        }

        private void InstallCamp()
        {
            Container.Bind<CampActionService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CampController>().AsSingle();
            Container.Bind<Events.Outcomes.IEventOutcomeScaler>()
                .To<Camp.CampEventOutcomeScaler>().AsSingle();
            Container.Bind<CampActionButtonFactory>().AsSingle();
        }

        private void InstallQuests()
        {
            Container.Bind<PlayerCityLocator>().AsSingle();
            Container.Bind<QuestRepository>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QuestAvailabilityService>().AsSingle().NonLazy();
            Container.Bind<QuestPendingEndingsService>().AsSingle();
            Container.Bind<QuestRewardApplier>().AsSingle();
            Container.BindInterfacesTo<QuestStageChecker>().AsSingle();
            Container.Bind<QuestCityIndicatorService>().AsSingle();
            Container.Bind<BuildingQuestSlotViewModel>().AsTransient();
            Container.Bind<UI.Screens.TargetSelection.Search.CityCardBuilder>().AsSingle();

            Container.Bind<HealerService>().AsSingle();
            Container.Bind<ArchiveService>().AsSingle();
            Container.Bind<TempleService>().AsSingle();
            Container.Bind<BarracksService>().AsSingle();
            Container.BindInterfacesAndSelfTo<JournalService>().AsSingle().NonLazy();
        }

        private void InstallWorldModifiers()
        {
            Container.Bind<WorldModifierRepository>().AsSingle();
            Container.Bind<ModifierEffectQuery>().AsSingle();
            Container.Bind<CurrentRoadResolver>().AsSingle();
            Container.BindInterfacesTo<WorldModifierSpawner>().AsSingle();
            Container.BindInterfacesTo<ModifierVisibilityTracker>().AsSingle();
        }

        private void InstallWorldLabels()
        {
            Container.BindInterfacesAndSelfTo<CityViewSpawner>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CityLabelSpawner>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RoadEffectIconSpawner>().AsSingle().NonLazy();
        }

        private void InstallWorldCanvas()
        {
            Container.BindInstance(_strategicBounds).AsSingle();
            Container.Bind<WorldCanvasFactory>().AsSingle();
            Container.Bind<WorldCanvas>()
                .FromMethod(ctx => ctx.Container.Resolve<WorldCanvasFactory>().Create())
                .AsSingle()
                .NonLazy();
            Container.Bind<FloatingRewardSpawner>().AsSingle();
        }

        private void InstallGameEnd()
        {
            Container.BindInterfacesAndSelfTo<RunStatsService>().AsSingle().NonLazy();
            Container.Bind<RunEndOutcomeApplier>().AsSingle();
        }

        private void InstallPathVisualization()
        {
            var pathMaterial = new Material(_pathShader);
            pathMaterial.color = Color.yellow;

            Container.Bind<PathLineRenderer>().AsSingle();
            Container.Bind<PathLineFactory>().AsSingle().WithArguments(pathMaterial);
            Container.Bind<IPathVisualizationService>()
                .To<PathVisualizationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PathVisualizationController>()
                .AsSingle().NonLazy();
        }

        private sealed class InputSceneSetup : IInitializable, IDisposable
        {
            private readonly InputRouter _input;
            private readonly LayerMask _mask;

            public InputSceneSetup(InputRouter input, LayerMask mask)
            {
                _input = input;
                _mask = mask;
            }

            public void Initialize() => _input.SetInteractableLayerMask(_mask);
            public void Dispose() => _input.SetInteractableLayerMask(default);
        }

        private sealed class TooltipThemeSetup : IInitializable
        {
            private readonly TooltipView _view;
            private readonly UiThemeService _theme;

            public TooltipThemeSetup(TooltipView view, UiThemeService theme)
            {
                _view = view;
                _theme = theme;
            }

            public void Initialize() => _view.gameObject.InitializeColorBinders(themeService: _theme);
        }
    }
}
