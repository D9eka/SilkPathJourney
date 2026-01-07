using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Input;
using Internal.Scripts.World.Camera;
using Internal.Scripts.World.VisualObjects;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using Internal.Scripts.World.State;
using Internal.Scripts.World.Visual;

namespace Internal.Scripts.World.Installers
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
