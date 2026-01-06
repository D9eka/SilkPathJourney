using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Input;
using Internal.Scripts.World.Camera;
using Internal.Scripts.World.VisualObjects;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.World
{
    public class WorldInstaller : MonoInstaller
    {
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private WorldStatesData _worldStatesData;
        [SerializeField] private List<MonoBehVisualObject> _visualObjects;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<CameraRig>().AsSingle().WithArguments(_camera, 1f);
            Container.BindInterfacesAndSelfTo<WorldStateController>().AsSingle()
                .WithArguments(_worldStatesData.ViewModesData).NonLazy();
            Container.BindInterfacesAndSelfTo<WorldVisualObjectsController>().AsSingle()
                .WithArguments(_visualObjects.Select(visualObjects => visualObjects as IVisualObject).ToList())
                .NonLazy();
            
        
            Container.BindInterfacesAndSelfTo<InputManager>()
                .AsSingle()
                .WithArguments(_camera)
                .NonLazy();
        }
    }
}