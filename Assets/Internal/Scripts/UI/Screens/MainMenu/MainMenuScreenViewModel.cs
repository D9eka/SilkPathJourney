using Internal.Scripts.Camera;
using Internal.Scripts.Installers;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine.SceneManagement;
using Zenject;

namespace Internal.Scripts.UI.Screens.MainMenu
{
    public sealed class MainMenuScreenViewModel : ScreenViewModelBase
    {
        private readonly ISaveService _saveService;
        private readonly SceneReference _gameScene;

        public MainMenuScreenViewModel(
            ISaveService saveService,
            [Inject(Id = SceneRefId.Game)] SceneReference gameScene)
        {
            _saveService = saveService;
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
            SceneManager.LoadScene(_gameScene.SceneName);
        }

        public void Continue()
        {
            SceneManager.LoadScene(_gameScene.SceneName);
        }
    }
}
