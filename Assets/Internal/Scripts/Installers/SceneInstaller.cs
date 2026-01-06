using Internal.Scripts.Input;
using Internal.Scripts.Road.Follower;
using Internal.Scripts.Road.Generator;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Orientation;
using Internal.Scripts.Road.Pathfinder;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [SerializeField] private RoadFollowerView _roadFollowerView;
        [Space] 
        [SerializeField] private Village[] _villages;
        [SerializeField] private Village _currentVillage;
        [Space]
        [SerializeField] private NodeAuthoring[] _nodes;
        [SerializeField] private EdgeAuthoring[] _edges;

        public override void InstallBindings()
        {
            Container.Bind<UnityEngine.Camera>().FromInstance(_mainCamera)
                .AsSingle()
                .NonLazy();
            
            Container.BindInterfacesTo<DijkstraRoadPathfinderStrategy>()
                .AsSingle();

            Container.BindInterfacesTo<Orientation2DStrategy>()
                .AsSingle()
                .WhenInjectedInto<RoadFollowerView>();
            
            Container.Bind<int>()
                .FromInstance(_currentVillage.RoadNodeIndex)
                .AsSingle()
                .WhenInjectedInto<RoadFollowerView>();

            Container.Bind<RoadFollowerView>()
                .FromInstance(_roadFollowerView)
                .AsSingle();
        
            Container.BindInterfacesAndSelfTo<InputManager>()
                .AsSingle()
                .NonLazy();

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
            
            RoadGraphGenerator graphGenerator = new RoadGraphGenerator();
            RoadGraph roadGraph = graphGenerator.BuildGraph(_nodes, _edges);
            Container.BindInstance(roadGraph)
                .AsSingle();
        }
    }
}
