using Internal.Scripts.Camera;
using Internal.Scripts.Installers;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.Utils;
using Zenject;

namespace Internal.Scripts.UI.Screens.MainMenu
{
    public sealed class MainMenuScreenViewModel : ScreenViewModelBase
    {
        private readonly ISaveService _saveService;
        private readonly SceneReference _gameScene;
        private readonly QuitGameService _quitGameService;
        private readonly SceneLoaderService _sceneLoader;

        public MainMenuScreenViewModel(
            ISaveService saveService,
            QuitGameService quitGameService,
            SceneLoaderService sceneLoader,
            [Inject(Id = SceneRefId.Game)] SceneReference gameScene)
        {
            _saveService = saveService;
            _quitGameService = quitGameService;
            _sceneLoader = sceneLoader;
            _gameScene = gameScene;
        }

        public override ScreenId Id => ScreenId.MainMenu;

        public bool HasSave => _saveService.HasSave();

        protected override void OnOpen(object args)
        {
        }

        protected override void OnClose()
        {
        }

        public void NewGame()
        {
            _saveService.Delete();
            _sceneLoader.LoadScene(_gameScene);
        }

        public void Continue()
        {
            _sceneLoader.LoadScene(_gameScene);
        }

        public void QuitGame()
        {
            _quitGameService.Quit();
        }
    }
}
