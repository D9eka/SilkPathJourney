using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Event;
using Internal.Scripts.UI.Screens.Hud;
using Internal.Scripts.UI.Screens.Inventory;
using Internal.Scripts.UI.Screens.MainMenu;
using Internal.Scripts.UI.Screens.Pause;
using Internal.Scripts.UI.Screens.TargetSelection;
using Internal.Scripts.UI.Screens.Trade;
using Internal.Scripts.UI.Screens.EnterCity;
using Internal.Scripts.UI.Screens.LanguageSchool;
using Internal.Scripts.UI.Screens.Quests;
using Internal.Scripts.UI.Screens.Trader;
using Internal.Scripts.UI.Screens.Caravan;
using Internal.Scripts.UI.Screens.Caravansary;
using Internal.Scripts.UI.Screens.Tavern;
using Internal.Scripts.UI.Screens.Workshop;
using Internal.Scripts.UI.Screens.Guild;
using Internal.Scripts.UI.Screens.CityEntryConfirm;
using Internal.Scripts.UI.Screens.Camp;
using Internal.Scripts.UI.Screens.HazardQte;
using Internal.Scripts.UI.Screens.NewGame;
using Internal.Scripts.UI.Screens.Legacy;
using Internal.Scripts.UI.Screens.RunEnd;
using Internal.Scripts.UI.Screens.TargetSelection.Search;
using Plugins.Zenject.Source.Main;

namespace Internal.Scripts.UI.Factory
{
    public sealed class ScreenViewModelFactory : IScreenViewModelFactory
    {
        private readonly DiContainer _container;

        public ScreenViewModelFactory(DiContainer container)
        {
            _container = container;
        }

        public ScreenViewModelBase Create(ScreenId id, IScreenView view)
        {
            return id switch
            {
                ScreenId.Hud => CreateHud(view),
                ScreenId.Inventory => CreateInventory(view),
                ScreenId.Trade => CreateTrade(view),
                ScreenId.Event => CreateEvent(view),
                ScreenId.TargetSelection => CreateTargetSelection(view),
                ScreenId.Pause => CreatePause(view),
                ScreenId.MainMenu => CreateMainMenu(view),
                ScreenId.Trader => CreateTrader(view),
                ScreenId.LanguageSchool => CreateLanguageSchool(view),
                ScreenId.EnterCity => CreateEnterCity(view),
                ScreenId.Quests => CreateQuests(view),
                ScreenId.Caravan => CreateCaravan(view),
                ScreenId.Caravansary => CreateCaravansary(view),
                ScreenId.Tavern => CreateTavern(view),
                ScreenId.Workshop => CreateWorkshop(view),
                ScreenId.Guild => CreateGuild(view),
                ScreenId.CityEntryConfirm => CreateCityEntryConfirm(view),
                ScreenId.Camp => CreateCamp(view),
                ScreenId.HazardQte => CreateHazardQte(view),
                ScreenId.NewGame => CreateNewGame(view),
                ScreenId.ConfirmNewGame => CreateConfirmNewGame(view),
                ScreenId.RunEnd => CreateRunEnd(view),
                ScreenId.Legacy => CreateLegacy(view),
                _ => null
            };
        }

        private ScreenViewModelBase CreateHud(IScreenView view)
        {
            if (view is not HudScreen)
                return null;

            return _container.Instantiate<HudScreenViewModel>();
        }

        private ScreenViewModelBase CreateInventory(IScreenView view)
        {
            if (view is not InventoryScreen)
                return null;

            return _container.Instantiate<InventoryScreenViewModel>();
        }

        private ScreenViewModelBase CreateTrade(IScreenView view)
        {
            if (view is not TradeScreen)
                return null;

            return _container.Instantiate<TradeScreenViewModel>();
        }

        private ScreenViewModelBase CreateEvent(IScreenView view)
        {
            if (view is not EventScreen)
                return null;

            return _container.Instantiate<EventScreenViewModel>();
        }

        private ScreenViewModelBase CreateTargetSelection(IScreenView view)
        {
            if (view is not TargetSelectionScreen)
                return null;

            var searchPanel = _container.Instantiate<CitySearchPanelViewModel>();
            return _container.Instantiate<TargetSelectionScreenViewModel>(new object[] { searchPanel });
        }

        private ScreenViewModelBase CreatePause(IScreenView view)
        {
            if (view is not PauseScreen)
                return null;

            return _container.Instantiate<PauseScreenViewModel>();
        }

        private ScreenViewModelBase CreateMainMenu(IScreenView view)
        {
            if (view is not MainMenuScreen)
                return null;

            return _container.Instantiate<MainMenuScreenViewModel>();
        }

        private ScreenViewModelBase CreateTrader(IScreenView view)
        {
            if (view is not TraderScreen)
                return null;

            return _container.Instantiate<TraderScreenViewModel>();
        }

        private ScreenViewModelBase CreateLanguageSchool(IScreenView view)
        {
            if (view is not LanguageSchoolScreen)
                return null;

            return _container.Instantiate<LanguageSchoolScreenViewModel>();
        }

        private ScreenViewModelBase CreateEnterCity(IScreenView view)
        {
            if (view is not EnterCityScreen)
                return null;

            return _container.Instantiate<EnterCityScreenViewModel>();
        }

        private ScreenViewModelBase CreateQuests(IScreenView view)
        {
            if (view is not QuestsScreen)
                return null;

            return _container.Instantiate<QuestsScreenViewModel>();
        }

        private ScreenViewModelBase CreateCaravan(IScreenView view)
        {
            if (view is not CaravanScreen)
                return null;

            return _container.Instantiate<CaravanScreenViewModel>();
        }

        private ScreenViewModelBase CreateCaravansary(IScreenView view)
        {
            if (view is not CaravansaryScreen)
                return null;

            return _container.Instantiate<CaravansaryScreenViewModel>();
        }

        private ScreenViewModelBase CreateTavern(IScreenView view)
        {
            if (view is not TavernScreen)
                return null;

            return _container.Instantiate<TavernScreenViewModel>();
        }

        private ScreenViewModelBase CreateWorkshop(IScreenView view)
        {
            if (view is not WorkshopScreen)
                return null;

            return _container.Instantiate<WorkshopScreenViewModel>();
        }

        private ScreenViewModelBase CreateGuild(IScreenView view)
        {
            if (view is not GuildScreen)
                return null;

            return _container.Instantiate<GuildScreenViewModel>();
        }

        private ScreenViewModelBase CreateCityEntryConfirm(IScreenView view)
        {
            if (view is not CityEntryConfirmScreen)
                return null;

            return _container.Instantiate<CityEntryConfirmScreenViewModel>();
        }

        private ScreenViewModelBase CreateCamp(IScreenView view)
        {
            if (view is not CampScreen)
                return null;

            return _container.Instantiate<CampScreenViewModel>();
        }

        private ScreenViewModelBase CreateHazardQte(IScreenView view)
        {
            if (view is not HazardQteScreen)
                return null;

            return _container.Instantiate<HazardQteViewModel>();
        }

        private ScreenViewModelBase CreateNewGame(IScreenView view)
        {
            if (view is not NewGameScreen)
                return null;

            return _container.Instantiate<NewGameScreenViewModel>();
        }

        private ScreenViewModelBase CreateConfirmNewGame(IScreenView view)
        {
            if (view is not ConfirmNewGameScreen)
                return null;

            return _container.Instantiate<ConfirmNewGameScreenViewModel>();
        }

        private ScreenViewModelBase CreateRunEnd(IScreenView view)
        {
            if (view is not RunEndScreen)
                return null;

            return _container.Instantiate<RunEndScreenViewModel>();
        }

        private ScreenViewModelBase CreateLegacy(IScreenView view)
        {
            if (view is not LegacyScreen)
                return null;

            return _container.Instantiate<LegacyScreenViewModel>();
        }
    }
}
