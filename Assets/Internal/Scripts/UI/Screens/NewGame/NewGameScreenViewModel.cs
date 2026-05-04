using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Camera;
using Internal.Scripts.Caravan;
using Internal.Scripts.Economy;
using Internal.Scripts.Installers;
using Internal.Scripts.Items;
using Internal.Scripts.Player.Background;
using Internal.Scripts.Meta;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.Utils;
using R3;
using Zenject;

namespace Internal.Scripts.UI.Screens.NewGame
{
    public sealed class NewGameScreenViewModel : ScreenViewModelBase
    {
        private readonly BackgroundDatabase _backgroundDatabase;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly ActiveSaveSlot _activeSaveSlot;
        private readonly SceneLoaderService _sceneLoader;
        private readonly SceneReference _gameScene;
        private readonly UiThemeService _themeService;
        private readonly LocalizationService _localization;
        private readonly ItemCatalog _itemCatalog;
        private readonly EconomyDatabase _economyDatabase;
        private readonly PersistentProgressService _persistent;

        public NewGameScreenViewModel(
            BackgroundDatabase backgroundDatabase,
            CaravanDatabase caravanDatabase,
            ActiveSaveSlot activeSaveSlot,
            SceneLoaderService sceneLoader,
            UiThemeService themeService,
            LocalizationService localization,
            ItemCatalog itemCatalog,
            EconomyDatabase economyDatabase,
            PersistentProgressService persistent,
            [Inject(Id = SceneRefId.Game)] SceneReference gameScene)
        {
            _backgroundDatabase = backgroundDatabase;
            _caravanDatabase = caravanDatabase;
            _activeSaveSlot = activeSaveSlot;
            _sceneLoader = sceneLoader;
            _themeService = themeService;
            _localization = localization;
            _itemCatalog = itemCatalog;
            _economyDatabase = economyDatabase;
            _persistent = persistent;
            _gameScene = gameScene;

            CanStart = Observable.CombineLatest(
                SelectedBackground,
                SelectedCartClass,
                (bg, cart) => bg != null && IsBackgroundUnlocked(bg) && cart != null
            ).ToReadOnlyReactiveProperty();
        }

        public UiThemeService ThemeService => _themeService;
        public LocalizationService Localization => _localization;
        public ItemCatalog ItemCatalog => _itemCatalog;
        public EconomyDatabase EconomyDatabase => _economyDatabase;

        public override ScreenId Id => ScreenId.NewGame;

        public IReadOnlyList<BackgroundData> Backgrounds => _backgroundDatabase.Backgrounds;
        public IReadOnlyList<CartClassData> CartClasses => _caravanDatabase.CartClasses;

        public ReactiveProperty<BackgroundData> SelectedBackground { get; } = new(null);
        public ReactiveProperty<CartClassData> SelectedCartClass { get; } = new(null);

        public ReadOnlyReactiveProperty<bool> CanStart { get; }

        public bool IsBackgroundUnlocked(BackgroundData data) =>
            data.IsUnlockedByDefault || _persistent.UnlockedIds.Contains($"unlock_background_{data.Id}");

        public void Start()
        {
            if (!CanStart.CurrentValue) return;
            _activeSaveSlot.CreateNew(SelectedBackground.Value.Id, SelectedCartClass.Value.Id);
            _sceneLoader.LoadScene(_gameScene);
        }

        protected override void OnOpen(object args)
        {
            BackgroundData firstUnlocked = _backgroundDatabase.GetFirstUnlocked();
            if (firstUnlocked != null)
                SelectedBackground.Value = firstUnlocked;

            if (_caravanDatabase.CartClasses.Count > 0)
                SelectedCartClass.Value = _caravanDatabase.CartClasses[0];
        }

        protected override void OnClose()
        {
            SelectedBackground.Value = null;
            SelectedCartClass.Value = null;
        }
    }
}
