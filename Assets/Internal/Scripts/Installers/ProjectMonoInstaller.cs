using Internal.Scripts.Input;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Tooltip;
using Plugins.Zenject.Source.Install;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.Installers
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private TooltipView _tooltipPrefab;

        public override void InstallBindings()
        {
            Container.Bind<UnityEngine.Camera>()
                .FromMethod(_ => UnityEngine.Camera.main)
                .AsTransient();

            Container.Bind<PlayerInputActions>().AsSingle();
            Container.BindInterfacesAndSelfTo<InputModeTracker>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InputRouter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UiInputRouter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QteInputRouter>().AsSingle().NonLazy();

            Container.BindInstance(new GroundSnapper(_groundLayerMask)).AsSingle();

            var tooltipCanvasGo = new GameObject("TooltipCanvas");
            DontDestroyOnLoad(tooltipCanvasGo);

            var tooltipCanvas = tooltipCanvasGo.AddComponent<Canvas>();
            tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tooltipCanvas.sortingOrder = 32000;
            tooltipCanvasGo.AddComponent<GraphicRaycaster>();

            Container.BindInstance(tooltipCanvas).WithId("TooltipCanvas").AsSingle();

            TooltipView tooltip = Instantiate(_tooltipPrefab, tooltipCanvas.transform);
            Container.BindInstance(tooltip).AsSingle();
            Container.BindInterfacesAndSelfTo<TooltipService>().AsSingle().NonLazy();
        }
    }
}
