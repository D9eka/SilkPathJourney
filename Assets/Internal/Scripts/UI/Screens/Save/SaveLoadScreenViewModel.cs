using System;
using System.Collections.Generic;
using Internal.Scripts.Camera;
using Internal.Scripts.Economy;
using Internal.Scripts.Installers;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.Utils;
using Zenject;

namespace Internal.Scripts.UI.Screens.Save
{
    public enum SaveLoadMode
    {
        Save,
        Load
    }

    public sealed class SaveLoadScreenViewModel : ScreenViewModelBase
    {
        private readonly ISaveService _saveService;
        private readonly SaveRepository _saveRepository;
        private readonly ActiveSaveSlot _activeSaveSlot;
        private readonly EconomyDatabase _economyDatabase;
        private readonly SceneLoaderService _sceneLoader;
        private readonly SceneReference _gameScene;
        private readonly ScreenStackService _screenStackService;
        private readonly ResourceIconCatalog _resourceIconCatalog;
        private readonly UiThemeService _themeService;
        private readonly StaticColorController _colorController;

        public event Action SlotsChanged;

        public SaveLoadMode Mode { get; private set; }
        public ResourceIconCatalog ResourceIcons => _resourceIconCatalog;
        public UiThemeService ThemeService => _themeService;
        public StaticColorController ColorController => _colorController;

        public override ScreenId Id => Mode == SaveLoadMode.Save ? ScreenId.SaveGame : ScreenId.LoadGame;

        public SaveLoadScreenViewModel(
            ISaveService saveService,
            SaveRepository saveRepository,
            ActiveSaveSlot activeSaveSlot,
            EconomyDatabase economyDatabase,
            SceneLoaderService sceneLoader,
            ScreenStackService screenStackService,
            ResourceIconCatalog resourceIconCatalog,
            UiThemeService themeService,
            StaticColorController colorController,
            [Inject(Id = SceneRefId.Game)] SceneReference gameScene)
        {
            _saveService = saveService;
            _saveRepository = saveRepository;
            _activeSaveSlot = activeSaveSlot;
            _economyDatabase = economyDatabase;
            _sceneLoader = sceneLoader;
            _screenStackService = screenStackService;
            _resourceIconCatalog = resourceIconCatalog;
            _themeService = themeService;
            _colorController = colorController;
            _gameScene = gameScene;
        }

        protected override void OnOpen(object args)
        {
            Mode = args is SaveLoadMode m ? m : SaveLoadMode.Load;
        }

        protected override void OnClose()
        {
        }

        public List<SaveMetadata> GetSaves()
        {
            return _saveService.GetAllSaves();
        }

        public string ResolveCityName(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return "";

            var city = _economyDatabase.Cities.Find(c => c.NodeId == nodeId);
            if (city == null)
                return "";

            return city.Name.GetLocalizedString();
        }

        public void PerformAction(string slotId)
        {
            if (Mode == SaveLoadMode.Save)
            {
                _activeSaveSlot.Set(slotId);
                _saveRepository.Save(isAutoSave: false);
                _screenStackService.CloseAllAbove(ScreenId.Hud);
            }
            else
            {
                _activeSaveSlot.Set(slotId);
                _sceneLoader.LoadScene(_gameScene);
            }
        }

        public void SaveToNewSlot()
        {
            _activeSaveSlot.CreateNew();
            _saveRepository.Save(isAutoSave: false);
            _screenStackService.CloseAllAbove(ScreenId.Hud);
        }

        public void DeleteSlot(string slotId)
        {
            _saveService.Delete(slotId);
            SlotsChanged?.Invoke();
        }
    }
}
