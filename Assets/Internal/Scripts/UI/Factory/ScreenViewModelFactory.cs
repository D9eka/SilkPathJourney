using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.View;
using Internal.Scripts.UI.Screen.ViewModel;
using Internal.Scripts.UI.Screens.Hud;
using Internal.Scripts.UI.Screens.Inventory;
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
    }
}
