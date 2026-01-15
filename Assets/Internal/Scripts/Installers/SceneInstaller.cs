using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Input;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using Internal.Scripts.World.State;
using Internal.Scripts.World.Village;
using Internal.Scripts.World.Visual;
using Internal.Scripts.World.VisualObjects;
using Plugins.Zenject.Source.Install;
using UnityEngine;

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
        [Header("Villages")]
        [SerializeField] private Village[] _villages;
        [SerializeField] private Village _currentVillage;
        [Header("NPC")]
        [SerializeField] private NpcSpawnEntry[] _spawns;
        [SerializeField] private NpcSimulationSettings _simulationSettings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputManager>()
                .AsSingle()
                .NonLazy();

            InstallCamera();
            InstallWorld();
            InstallNpc();
        }

        private void InstallCamera()
        {
            Container.Bind<UnityEngine.Camera>().FromInstance(_mainCamera)
                .AsSingle()
                .NonLazy();

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

        public void InstallVillage()
        {
            Container.Bind<Village[]>()
                .FromInstance(_villages)
                .AsSingle();

            Container.Bind<Village>()
                .WithId("CurrentVillage")
                .FromInstance(_currentVillage)
                .AsSingle();

            Container.BindInterfacesAndSelfTo<VillageNavigator>()
                .AsSingle()
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
