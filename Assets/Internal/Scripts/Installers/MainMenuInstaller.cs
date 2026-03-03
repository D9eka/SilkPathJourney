using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI;
using Internal.Scripts.UI.Factory;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.Utils;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    public class MainMenuInstaller : MonoInstaller
    {
        [SerializeField] private UIScreenRoots _uiScreenRoots;
        [SerializeField] private BiomePaletteMap _biomePaletteMap;

        public override void InstallBindings()
        {
            if (_uiScreenRoots != null)
                Container.BindInstance(_uiScreenRoots).AsSingle();

            Container.BindInstance(_biomePaletteMap).AsSingle();

            _biomePaletteMap.TryGetPalette(Biome.Plains, out var defaultPalette);
            Container.Bind<UiThemeService>().AsSingle().WithArguments(defaultPalette);
            Container.Bind<StaticColorController>().AsSingle();

            Container.Bind<SceneLoaderService>().AsSingle();
            Container.Bind<IScreenViewModelFactory>().To<ScreenViewModelFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScreenStackService>().AsSingle()
                .WithArguments(ScreenId.MainMenu);
            Container.BindInterfacesTo<ScreenBackNavigator>().AsSingle();
        }
    }
}
