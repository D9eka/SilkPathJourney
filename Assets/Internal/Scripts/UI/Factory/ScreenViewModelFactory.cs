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

            return _container.Instantiate<TargetSelectionScreenViewModel>();
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
    }
}
