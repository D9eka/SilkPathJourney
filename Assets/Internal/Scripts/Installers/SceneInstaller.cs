using System;
using System.Linq;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Input;
using Internal.Scripts.Inventory;
using Internal.Scripts.Hud;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Nodes.UI;
using Internal.Scripts.Road.Nodes.UI.NodesViewer;
using Internal.Scripts.Road.Path;
using Internal.Scripts.World.State;
using Internal.Scripts.World.Visual;
using Internal.Scripts.World.VisualObjects;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Save;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI;
using Internal.Scripts.UI.Arrow;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Arrow.DirectionCalculation;
using Internal.Scripts.UI.Arrow.JunctionBalancer;
using Internal.Scripts.UI.Arrow.Placement;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Factory;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screens.Config;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.Trading;

namespace Internal.Scripts.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [Header("Camera")]
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [SerializeField] private CameraZoomerData _cameraZoomerData;
        [Space]
        [Header("World")]
        [SerializeField] private WorldStatesData _worldStatesData;
        [SerializeField] private NodeView _nodeViewPrefab;
        [Space]
        [Header("Economy")]
        [SerializeField] private EconomyDatabase _economyDatabase;
        [SerializeField] private EconomySimulationSettings _economySimulationSettings;
        [Space]
        [Header("NPC")]
        [SerializeField] private NpcSpawnEntry[] _spawns;
        [SerializeField] private NpcSimulationSettings _simulationSettings;
        [Header("Player")]
        [SerializeField] private RoadAgentView _playerViewPrefab;
        [SerializeField] private RoadAgentConfig _playerAgentConfig;
        [SerializeField] private PlayerConfig _playerProfile;
        [Header("UI Screens")]
        [SerializeField] private UIScreenRoots _uiScreenRoots;
        [SerializeField] private ScreenCatalog _screenCatalog;
        [Header("Interactables")]
        [SerializeField] private LayerMask _interactableLayerMask;
        [SerializeField] private LayerMask _groundLayerMask;
        [Header("Arrows")]
        [SerializeField] private Transform _arrowsRoot;
        [SerializeField] private ArrowView _arrowPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputManager>()
                .AsSingle().WithArguments(_interactableLayerMask)
                .NonLazy();

            InstallCamera();
            InstallWorld();
            InstallRoad();
            InstallNpc();
            BindPlayerConfig();
            InstallEconomy();
            InstallPlayer();
            InstallScreens();
        }

        private void InstallCamera()
        {
            Container.Bind<UnityEngine.Camera>().FromInstance(_mainCamera)
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<CameraZoomer>().AsSingle().WithArguments(_cameraZoomerData);
            Container.BindInterfacesTo<CameraMover>().AsSingle();
        }

        public void InstallWorld()
        {
            MonoBehVisualObject[] visualObjects = FindObjectsByType<MonoBehVisualObject>(FindObjectsSortMode.None);
            Container.BindInterfacesAndSelfTo<WorldStateController>().AsSingle()
                .WithArguments(_worldStatesData.ViewModesData).NonLazy();
            Container.BindInterfacesAndSelfTo<WorldVisualObjectsController>().AsSingle()
                .WithArguments(visualObjects.Select(visualObjects => visualObjects as IVisualObject).ToList())
                .NonLazy();
        }
        
        private void InstallRoad()
        {
            RoadRuntime[] roads = FindObjectsByType<RoadRuntime>(FindObjectsSortMode.None);

            Container.Bind<RoadPoseSampler>().AsSingle();
            Container.BindInstance(roads).AsSingle();
            Container.Bind<RoadSamplerCache>().AsSingle();

            Container.BindInterfacesAndSelfTo<RoadNodeLookup>().AsSingle().NonLazy();
            Container.BindInterfacesTo<NodesViewer>().AsSingle().WithArguments(_nodeViewPrefab);
            
            Container.BindInterfacesAndSelfTo<RoadNetwork>().AsSingle().NonLazy();
            Container.Bind<IRoadPathFinder>().To<RoadPathFinder>().AsSingle();
        }
        
        private void InstallNpc()
        {
            Container.BindInterfacesAndSelfTo<NpcSimulation>().AsSingle();
            Container.Bind<NpcFactory>().AsSingle();

            Container.BindInstance(_spawns ?? Array.Empty<NpcSpawnEntry>())
                .WhenInjectedInto<NpcBootstrapper>();
            Container.BindInterfacesAndSelfTo<NpcBootstrapper>().AsSingle().NonLazy();

            if (_simulationSettings != null)
            {
                Container.BindInstance(_simulationSettings).AsSingle();
                Container.BindInterfacesAndSelfTo<NpcLifeSimulator>().AsSingle().NonLazy();
            }
        }

        private void InstallPlayer()
        {
            InstallArrows();
            Container.BindInterfacesTo<PlayerChoiceInputView>().AsSingle();
            Container.Bind<PathHintsCreator>().AsSingle();
            Container.Bind<RoadAgentView>().FromComponentInNewPrefab(_playerViewPrefab).AsSingle()
                .WhenInjectedInto<PlayerInitializer>();
            Container.Bind<RoadAgentConfig>().FromInstance(_playerAgentConfig).AsSingle()
                .WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesAndSelfTo<SegmentMover>().AsSingle().WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesAndSelfTo<PlayerNextSegmentsProvider>().AsSingle();
            Container.BindInterfacesTo<PlayerStartMovement>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerSaveController>().AsSingle();
            Container.BindInterfacesTo<PlayerInitializer>().AsSingle();
            Container.BindInterfacesTo<CityNodeResolver>().AsSingle();
        }

        private void InstallEconomy()
        {
            Container.BindInstance(_economyDatabase).AsSingle(); 
            Container.BindInstance(_economySimulationSettings).AsSingle();

            Container.Bind<ISaveService>().To<JsonSaveService>().AsSingle();
            Container.Bind<SaveRepository>().AsSingle();
            Container.Bind<EconomySaveBuilder>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveBootstrapper>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InventoryRepository>().AsSingle().NonLazy();
        }

        private void InstallScreens()
        {
            if (_uiScreenRoots != null)
                Container.BindInstance(_uiScreenRoots).AsSingle();
            if (_screenCatalog != null)
                Container.BindInstance(_screenCatalog).AsSingle();

            Container.Bind<InventoryModel>().AsSingle();
            Container.Bind<TradeModel>().AsSingle();
            Container.Bind<HudModel>().AsSingle();

            Container.Bind<IScreenViewModelFactory>().To<ScreenViewModelFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScreenStackService>().AsSingle().WithArguments(ScreenId.Hud);
            Container.BindInterfacesTo<ScreenBackHandler>().AsSingle();
        }

        private void BindPlayerConfig()
        {
            Container.BindInstance(_playerProfile).AsSingle();
        }
        
        private void InstallArrows()
        {
            Container.Bind<GroundSnapper>().AsSingle()
                .WithArguments(_groundLayerMask);
            
            Container.Bind<IArrowPositionCalculator>()
                .To<RoadPoseArrowPositionCalculator>().AsSingle();

            Container.Bind<IArrowDirectionCalculator>()
                .To<RoadPoseArrowDirectionCalculator>().AsSingle();
            
            Container.BindInterfacesTo<ArrowJunctionBalancer>().AsSingle();

            Container.Bind<IArrowPlacementService>()
                .To<ArrowPlacementService>().AsSingle()
                .WithArguments(_arrowsRoot, _arrowPrefab);

            Container.BindInterfacesTo<RoadPoseArrowsController>()
                .AsSingle();
        }
    }
}
