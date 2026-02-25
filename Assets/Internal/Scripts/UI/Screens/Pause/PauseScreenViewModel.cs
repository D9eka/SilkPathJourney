using Internal.Scripts.Camera;
using Internal.Scripts.Installers;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.World.State;
using UnityEngine.SceneManagement;
using Zenject;

namespace Internal.Scripts.UI.Screens.Pause
{
    public sealed class PauseScreenViewModel : ScreenViewModelBase
    {
        private readonly GameClock _gameClock;
        private readonly SceneReference _mainMenuScene;

        public PauseScreenViewModel(
            GameClock gameClock,
            [Inject(Id = SceneRefId.MainMenu)] SceneReference mainMenuScene)
        {
            _gameClock = gameClock;
            _mainMenuScene = mainMenuScene;
        }

        public override ScreenId Id => ScreenId.Pause;

        protected override void OnOpen(object args)
        {
            _gameClock.Pause();
        }

        protected override void OnClose()
        {
            _gameClock.Resume();
        }

        public void ExitToMenu()
        {
            _gameClock.Resume();
            SceneManager.LoadScene(_mainMenuScene.SceneName);
        }
    }
}
