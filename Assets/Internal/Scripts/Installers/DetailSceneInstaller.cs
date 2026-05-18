using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Tooltip;
using Internal.Scripts.UI.WorldLabel;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    public class DetailSceneInstaller : MonoInstaller
    {
        [SerializeField] private DetailSceneBounds _bounds;
        [SerializeField] private WorldCanvasSettings _worldCanvasSettings;

        public override void InstallBindings()
        {
            Container.Bind<WorldCanvas>()
                .FromMethod(ctx =>
                {
                    var factory = new WorldCanvasFactory(
                        _bounds,
                        ctx.Container.Resolve<TooltipService>(),
                        ctx.Container.Resolve<LocalizationService>(),
                        ctx.Container.Resolve<GroundSnapper>(),
                        ctx.Container.Resolve<UnityEngine.Camera>(),
                        _worldCanvasSettings,
                        ctx.Container.Resolve<CameraZoomerData>(),
                        ctx.Container.Resolve<ScreenStackService>());
                    return factory.Create();
                })
                .AsSingle();
        }
    }
}
