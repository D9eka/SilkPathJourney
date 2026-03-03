using System;
using System.Linq;
using Internal.Scripts.Camera;
using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Tilt;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Input;
using Internal.Scripts.Inventory;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Encounter;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.State;
using Internal.Scripts.Road.Path;
using Internal.Scripts.World.State;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using Zenject;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Save;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI;
using Internal.Scripts.UI.Arrow;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Arrow.DirectionCalculation;
using Internal.Scripts.UI.Arrow.JunctionBalancer;
using Internal.Scripts.UI.Arrow.Placement;
using Internal.Scripts.UI.Factory;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.Trading;
using Internal.Scripts.Events;
using Internal.Scripts.Items;
using Internal.Scripts.UI.WorldLabel;
using Internal.Scripts.Road.Positioning;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.PathVisualization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Hud;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Utils;

namespace Internal.Scripts.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [Header("Camera")]
        [SerializeField] private DetailSceneBounds _strategicBounds;
        [Header("World")]
        [Header("NPC")]
        [SerializeField] private NpcSpawnEntry[] _spawns;
        [SerializeField] private NpcEncounterSettings _npcEncounterSettings;
        [Header("Player")]
        [SerializeField] private RoadAgentView _playerViewPrefab;
        [SerializeField] private RoadAgentConfig _playerAgentConfig;
        [Header("UI Screens")]
        [SerializeField] private UIScreenRoots _uiScreenRoots;
        [Header("Interactables")]
        [SerializeField] private LayerMask _interactableLayerMask;
        [Header("Arrows")]
        [SerializeField] private ArrowView _arrowPrefab;
        [Header("World Labels")]
        [SerializeField] private WorldCanvasSettings _worldCanvasSettings;
        [Header("Path Visualization")]
        [SerializeField] private Shader _pathShader;
        [Header("UI Theme")]
        [SerializeField] private BiomePaletteMap _biomePaletteMap;

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
            InstallNpc();
            InstallEconomy();
            InstallWorldCanvas();
            InstallWorldLabels();
            InstallPlayer();
            InstallTheme();
            InstallScreens();
            InstallEvents();
            InstallPathVisualization();

            Container.BindInterfacesTo<CameraSaveController>().AsSingle();
        }

        private void InstallCamera()
        {
            Container.Bind<CameraBounds>().AsSingle()
                .WithArguments(_strategicBounds.BoundsCollider, _strategicBounds.CenterTransform);

            Container.BindInterfacesTo<CameraZoomer>().AsSingle();
            Container.BindInterfacesTo<CameraTilter>().AsSingle();
            Container.BindInterfacesTo<CameraMover>().AsSingle();
            Container.BindInterfacesTo<CameraAutoFitter>().AsSingle();
            Container.Bind<MainSceneVisibilityController>().AsSingle();
            Container.Bind<DetailSceneLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraSceneLoader>().AsSingle();

            Container.Bind<CameraController>().AsSingle();
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
            Container.Bind<NpcTrader>().AsSingle();
            Container.BindInterfacesTo<NpcSaveController>().AsSingle();

            if (_npcEncounterSettings != null)
            {
                Container.BindInstance(_npcEncounterSettings).AsSingle();
                Container.BindInterfacesTo<NpcEncounterTrigger>().AsSingle();
            }
        }

        private void InstallPlayer()
        {
            InstallArrows();
            Container.BindInterfacesTo<PlayerChoiceInputView>().AsSingle();
            Container.Bind<PathHintsCreator>().AsSingle();
            Container.Bind<RoadAgentView>().FromComponentInNewPrefab(_playerViewPrefab).AsSingle()
                .WhenInjectedInto<PlayerInitializer>();
            Container.Bind<RoadAgentConfig>().FromInstance(_playerAgentConfig).AsSingle();
            Container.BindInterfacesAndSelfTo<SegmentMover>().AsSingle().WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesAndSelfTo<PlayerNextSegmentsProvider>().AsSingle();
            Container.BindInterfacesTo<PlayerStartMovement>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerSaveController>().AsSingle();
            Container.Bind<OverloadCalculator>().AsSingle();
            Container.Bind<CaravanSpeedService>().AsSingle();
            Container.Bind<DailyTravelCosts>().AsSingle();
            Container.BindInterfacesTo<PlayerInitializer>().AsSingle();
            Container.BindInterfacesTo<CityNodeResolver>().AsSingle();
        }

        private void InstallEconomy()
        {
            Container.Bind<ItemCatalog>().AsSingle();
            Container.Bind<ItemWeightCalculator>().AsSingle();
            Container.Bind<EconomySaveBuilder>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveBootstrapper>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InventoryRepository>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerResourceRepository>().AsSingle().NonLazy();

            Container.Bind<CityMarketProfileService>().AsSingle();
            Container.Bind<CityTradePriceService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CityEconomySimulator>().AsSingle().NonLazy();
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

            Container.Bind<Events.Conditions.ResourceConditionHandler>().AsSingle();
            Container.Bind<Events.Conditions.InventoryConditionHandler>().AsSingle();
            Container.Bind<Events.Conditions.CartConditionHandler>().AsSingle();
            Container.Bind<Events.Conditions.LocationConditionHandler>().AsSingle();
            Container.Bind<Events.Conditions.CompanionConditionHandler>().AsSingle();
            Container.Bind<Events.Conditions.SkillConditionHandler>().AsSingle();
            Container.Bind<Events.Conditions.ConditionEvaluator>().AsSingle();

            Container.Bind<Events.Outcomes.ResourceOutcomeHandler>().AsSingle();
            Container.Bind<Events.Outcomes.ItemOutcomeHandler>().AsSingle();
            Container.Bind<Events.Outcomes.CartDurabilityOutcomeHandler>().AsSingle();
            Container.Bind<Events.Outcomes.CompanionOutcomeHandler>().AsSingle();
            Container.Bind<Events.Outcomes.SkillOutcomeHandler>().AsSingle();
            Container.Bind<Events.Outcomes.RoadUnlockOutcomeHandler>().AsSingle();
            Container.Bind<Events.Outcomes.OutcomeApplier>().AsSingle();

            Container.Bind<EventToastController>().AsSingle();
            Container.BindInterfacesAndSelfTo<EventTrigger>().AsSingle().NonLazy();
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
            Container.BindInstance(_worldCanvasSettings).AsSingle();
            Container.Bind<WorldCanvasFactory>().AsSingle();
            Container.Bind<WorldCanvas>()
                .FromMethod(ctx => ctx.Container.Resolve<WorldCanvasFactory>().Create())
                .AsSingle()
                .NonLazy();
        }

        private void InstallTheme()
        {
            Container.BindInstance(_biomePaletteMap).AsSingle();

            _biomePaletteMap.TryGetPalette(Biome.Plains, out var defaultPalette);
            Container.Bind<UiThemeService>().AsSingle()
                .WithArguments(defaultPalette).NonLazy();

            Container.BindInterfacesTo<BiomeThemeController>().AsSingle().NonLazy();

            Container.Bind<StaticColorController>().AsSingle();
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
    }
}
