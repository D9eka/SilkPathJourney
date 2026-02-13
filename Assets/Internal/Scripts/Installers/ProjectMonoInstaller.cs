using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Tooltip;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        [SerializeField] private LayerMask _groundLayerMask;

        public override void InstallBindings()
        {
            Container.Bind<UnityEngine.Camera>()
                .FromMethod(_ => UnityEngine.Camera.main)
                .AsSingle();

            Container.BindInstance(new GroundSnapper(_groundLayerMask)).AsSingle();

            Container.BindInstance(TooltipView.Create()).AsSingle();
            Container.Bind<TooltipService>().AsSingle();
        }
    }
}
