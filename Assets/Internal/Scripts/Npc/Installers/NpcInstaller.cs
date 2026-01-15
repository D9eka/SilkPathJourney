using System;
using System.Linq;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Installers
{
    public class NpcInstaller : MonoInstaller
    {
        [SerializeField] private RoadRuntime[] _roadRuntimes;
        [SerializeField] private NpcSpawnEntry[] _spawns;
        [SerializeField] private NpcSimulationSettings _simulationSettings;

        public override void InstallBindings()
        {
            RoadRuntime[] roads = ResolveRoads();

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

        private RoadRuntime[] ResolveRoads()
        {
            if (_roadRuntimes != null && _roadRuntimes.Length > 0)
                return _roadRuntimes.Where(r => r != null).ToArray();

            return FindObjectsByType<RoadRuntime>(FindObjectsSortMode.None);
        }
    }
}
