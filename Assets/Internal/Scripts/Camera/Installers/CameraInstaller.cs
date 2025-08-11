using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using Plugins.Zenject.Source.Install;

namespace Internal.Scripts.Camera.Installers
{
    public class CameraInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ICameraMover>().To<CameraMover>().AsSingle();
            Container.Bind<ICameraZoomer>().To<CameraZoomer>().AsSingle();
            Container.Bind<ICameraAutoFitter>().To<CameraAutoFitter>().AsSingle();

            Container.Bind<CameraController>().AsSingle();
        }
    }
}