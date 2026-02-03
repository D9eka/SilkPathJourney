using System;
using Internal.Scripts.Input;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screens.Config;
using Zenject;

namespace Internal.Scripts.UI.StackService
{
    public sealed class ScreenBackHandler : IInitializable, IDisposable
    {
        private readonly ScreenStackService _stackService;
        private readonly ScreenCatalog _catalog;
        private readonly InputManager _inputManager;

        public ScreenBackHandler(ScreenStackService stackService, ScreenCatalog catalog, InputManager inputManager)
        {
            _stackService = stackService;
            _catalog = catalog;
            _inputManager = inputManager;
        }

        public void Initialize()
        {
            _inputManager.OnUiBack += HandleBack;
        }

        public void Dispose()
        {
            _inputManager.OnUiBack -= HandleBack;
        }

        private void HandleBack()
        {
            ScreenId topId = _stackService.TopId;
            if (topId is ScreenId.None) return;

            if (_catalog != null && _catalog.TryGet(topId, out ScreenConfig config) && config != null)
            {
                if (!config.CloseOnBack)
                    return;
            }

            _stackService.CloseTop();
        }
    }
}
