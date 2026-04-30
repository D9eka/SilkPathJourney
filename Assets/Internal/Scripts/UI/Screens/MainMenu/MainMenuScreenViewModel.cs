using Internal.Scripts.Camera;
using Internal.Scripts.Installers;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.Utils;
using R3;
using Zenject;

namespace Internal.Scripts.UI.Screens.MainMenu
{
    public sealed class MainMenuScreenViewModel : ScreenViewModelBase
    {
        private readonly ISaveService _saveService;
        private readonly ActiveSaveSlot _activeSaveSlot;
        private readonly SceneReference _gameScene;
        private readonly QuitGameService _quitGameService;
        private readonly SceneLoaderService _sceneLoader;
        private readonly ScreenStackService _screenStackService;

        private readonly ReactiveProperty<bool> _hasActiveRun = new(false);

        public Observable<bool> HasActiveRun => _hasActiveRun;

        public MainMenuScreenViewModel(
            ISaveService saveService,
            ActiveSaveSlot activeSaveSlot,
            QuitGameService quitGameService,
            SceneLoaderService sceneLoader,
            ScreenStackService screenStackService,
            [Inject(Id = SceneRefId.Game)] SceneReference gameScene)
        {
            _saveService = saveService;
            _activeSaveSlot = activeSaveSlot;
            _quitGameService = quitGameService;
            _sceneLoader = sceneLoader;
            _screenStackService = screenStackService;
            _gameScene = gameScene;
        }

        public override ScreenId Id => ScreenId.MainMenu;

        protected override void OnOpen(object args)
        {
            _hasActiveRun.Value = _saveService.HasActiveRun();
        }

        protected override void OnClose()
        {
        }

        public void NewGame()
        {
            if (_hasActiveRun.Value)
            {
                _screenStackService.TryOpen(ScreenId.ConfirmNewGame, (System.Action)NewGameConfirmed, out _);
                return;
            }

            _screenStackService.TryOpen(ScreenId.NewGame, out _);
        }

        private void NewGameConfirmed()
        {
            _saveService.DeleteRun();
            _hasActiveRun.Value = false;
            _screenStackService.TryOpen(ScreenId.NewGame, out _);
        }

        public void Continue()
        {
            if (!_hasActiveRun.Value)
                return;

            _activeSaveSlot.Set(JsonSaveService.RunSlotId);
            _sceneLoader.LoadScene(_gameScene);
        }

        public void OpenLegacyShop()
        {
            _screenStackService.TryOpen(ScreenId.Legacy, out _);
        }

        public void QuitGame()
        {
            _quitGameService.Quit();
        }
    }
}
