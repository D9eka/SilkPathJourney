using System;
using System.Linq;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Input;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Player.UI.Arrow;
using Internal.Scripts.Player.UI.Arrow.Controller;
using Internal.Scripts.Player.UI.Arrow.DirectionCalculation;
using Internal.Scripts.Player.UI.Arrow.JunctionBalancer;
using Internal.Scripts.Player.UI.Arrow.Placement;
using Internal.Scripts.Player.UI.Arrow.PositionCalculation;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using Internal.Scripts.World.State;
using Internal.Scripts.World.Visual;
using Internal.Scripts.World.VisualObjects;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using UnityEngine.UI;

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
        [Space]
        [Header("NPC")]
        [SerializeField] private NpcSpawnEntry[] _spawns;
        [SerializeField] private NpcSimulationSettings _simulationSettings;
        [Header("Player")]
        [SerializeField] private RoadAgentView _playerViewPrefab;
        [SerializeField] private RoadAgentConfig _playerConfig;
        [SerializeField] private Button _startMovementButton;
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
            InstallNpc();
            InstallPlayer();
        }
        
        private void InstallPlayer()
        {
            InstallArrows();
            Container.BindInterfacesTo<PlayerChoiceInputView>().AsSingle();
            Container.Bind<PathHintsCreator>().AsSingle();
            Container.Bind<RoadAgentView>().FromComponentInNewPrefab(_playerViewPrefab).AsSingle()
                .WhenInjectedInto<PlayerInitializer>();
            Container.Bind<RoadAgentConfig>().FromInstance(_playerConfig).AsSingle()
                .WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesAndSelfTo<SegmentMover>().AsSingle().WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesTo<PlayerNextSegmentsProvider>().AsSingle().WhenInjectedInto<PlayerInitializer>();
            Container.BindInterfacesTo<PlayerInitializer>().AsSingle()
                .WithArguments("N_Village_01","N_Village_04");
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
        
        private void InstallNpc()
        {
            RoadRuntime[] roads = FindObjectsByType<RoadRuntime>(FindObjectsSortMode.None);

            Container.Bind<RoadPoseSampler>().AsSingle();
            Container.BindInstance(roads).AsSingle();
            Container.Bind<RoadSamplerCache>().AsSingle();

            Container.BindInterfacesAndSelfTo<RoadNodeLookup>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RoadNetwork>().AsSingle().NonLazy();
            Container.Bind<IRoadPathFinder>().To<RoadPathFinder>().AsSingle();

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
    }
}
