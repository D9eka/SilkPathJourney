using Internal.Scripts.Input;
using Internal.Scripts.InteractableObjects;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [Space] 
        [SerializeField] private Village[] _villages;
        [SerializeField] private Village _currentVillage;

        public override void InstallBindings()
        {
            Container.Bind<UnityEngine.Camera>().FromInstance(_mainCamera)
                .AsSingle()
                .NonLazy();
        
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
        }
    }
}
