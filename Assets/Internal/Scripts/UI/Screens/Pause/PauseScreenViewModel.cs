using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Installers;
using Internal.Scripts.Meta;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.Utils;
using Internal.Scripts.World.State;
using Zenject;

namespace Internal.Scripts.UI.Screens.Pause
{
    public sealed class PauseScreenViewModel : ScreenViewModelBase
    {
        private readonly GameClock _gameClock;
        private readonly SceneReference _mainMenuScene;
        private readonly QuitGameService _quitGameService;
        private readonly SceneLoaderService _sceneLoader;
        private readonly ScreenStackService _screenStackService;
        private readonly RunEndOutcomeApplier _runEndApplier;

        public PauseScreenViewModel(
            GameClock gameClock,
            QuitGameService quitGameService,
            SceneLoaderService sceneLoader,
            ScreenStackService screenStackService,
            RunEndOutcomeApplier runEndApplier,
            [Inject(Id = SceneRefId.MainMenu)] SceneReference mainMenuScene)
        {
            _gameClock = gameClock;
            _quitGameService = quitGameService;
            _sceneLoader = sceneLoader;
            _screenStackService = screenStackService;
            _runEndApplier = runEndApplier;
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
            _sceneLoader.LoadScene(_mainMenuScene);
        }

        public void OnSettings()
        {
            _screenStackService.TryOpen(ScreenId.Settings, out _);
        }

        public void QuitGame()
        {
            _quitGameService.Quit();
        }

        public void EndRun()
        {
            Action onConfirmed = () => _runEndApplier.TriggerRunEnd(EndType.Defeat_CaravanDisbanded);
            _screenStackService.TryOpen(ScreenId.ConfirmEndRun, (System.Action)onConfirmed, out _);
        }
    }
}
