using System.Collections.Generic;
using Internal.Scripts.Camera;
using Internal.Scripts.Installers;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Save;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.Utils;
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

        public bool HasSave => _saveService.HasAnySave();

        protected override void OnOpen(object args)
        {
        }

        protected override void OnClose()
        {
        }

        public void NewGame()
        {
            _activeSaveSlot.CreateNew();
            _sceneLoader.LoadScene(_gameScene);
        }

        public void Continue()
        {
            List<SaveMetadata> saves = _saveService.GetAllSaves();
            if (saves.Count == 0)
                return;

            _activeSaveSlot.Set(saves[0].SlotId);
            _sceneLoader.LoadScene(_gameScene);
        }

        public void OpenLoad()
        {
            _screenStackService.TryOpen(ScreenId.LoadGame, SaveLoadMode.Load, out _);
        }

        public void QuitGame()
        {
            _quitGameService.Quit();
        }
    }
}
