using Internal.Scripts.Camera;
using Internal.Scripts.Input;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts
{
    public class MainInstaller : MonoInstaller
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
            
            Container.Bind<CameraController>().FromInstance(_mainCamera.GetComponent<CameraController>())
                .AsSingle()
                .NonLazy();
        
            Container.BindInterfacesAndSelfTo<InputManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<MoverController>()
                .AsSingle()
                .WithArguments(_villages, _currentVillage)
                .NonLazy();
        }
    }
}
