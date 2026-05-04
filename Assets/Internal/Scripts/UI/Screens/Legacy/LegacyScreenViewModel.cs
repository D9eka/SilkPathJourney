using Internal.Scripts.Items;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Legacy.Tabs.Achievement;
using Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats;
using Internal.Scripts.UI.Screens.Legacy.Tabs.Shop;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.Meta;
using Internal.Scripts.Meta.Achievements;
using Internal.Scripts.Meta.Unlocks;
using R3;

namespace Internal.Scripts.UI.Screens.Legacy
{
    public sealed class LegacyScreenViewModel : ScreenViewModelBase
    {
        private readonly LegacyShopTabViewModel _shopTab;
        private readonly LegacyAchievementsTabViewModel _achievementsTab;
        private readonly LegacyLifetimeTabViewModel _lifetimeTab;
        
        public readonly UiThemeService ThemeService;

        private readonly ReactiveProperty<LegacyViewState> _state = new();
        private LegacyTab _defaultTab = LegacyTab.Shop;
        private LegacyTab _activeTab = LegacyTab.Shop;

        public Observable<LegacyViewState> State => _state;
        public override ScreenId Id => ScreenId.Legacy;
        public LegacyShopTabViewModel ShopTab => _shopTab;
        public LegacyAchievementsTabViewModel AchievementsTab => _achievementsTab;
        public LegacyLifetimeTabViewModel LifetimeTab => _lifetimeTab;

        public LegacyScreenViewModel(
            PersistentProgressService persistent,
            UnlockRepository unlockRepository,
            AchievementDatabase achievementDatabase,
            ItemCatalog itemCatalog,
            UiThemeService themeService)
        {
            _shopTab = new LegacyShopTabViewModel(persistent, unlockRepository, themeService);
            _achievementsTab = new LegacyAchievementsTabViewModel(persistent, achievementDatabase, themeService);
            _lifetimeTab = new LegacyLifetimeTabViewModel(persistent, itemCatalog);
            ThemeService = themeService;
        }

        public void SetDefaultTab(LegacyTab tab)
        {
            _defaultTab = tab;
        }

        protected override void OnOpen(object args)
        {
            _activeTab = _defaultTab;
            BuildState(_activeTab);
        }

        protected override void OnClose()
        {
            _state.Value = null;
        }

        public void SwitchTab(LegacyTab tab)
        {
            if (!IsOpen) return;
            _activeTab = tab;
            BuildState(_activeTab);
        }

        public void TryUnlock(string unlockId)
        {
            if (_shopTab.TryUnlock(unlockId))
                BuildState(_activeTab);
        }

        private void BuildState(LegacyTab tab)
        {
            object tabState = BuildTabState(tab);
            int legacyPointsTotal = tabState is LegacyShopTabState shopState
                ? shopState.LegacyPointsTotal
                : _shopTab.LegacyPointsTotal;

            _state.Value = new LegacyViewState(legacyPointsTotal, tab, tabState);
        }

        private object BuildTabState(LegacyTab tab)
        {
            return tab switch
            {
                LegacyTab.Shop => _shopTab.BuildState(),
                LegacyTab.Achievements => _achievementsTab.BuildState(),
                LegacyTab.Lifetime => _lifetimeTab.BuildState(),
                _ => _shopTab.BuildState()
            };
        }
    }
}
