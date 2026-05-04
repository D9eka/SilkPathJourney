using System;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;

namespace Internal.Scripts.UI.Screens.MainMenu
{
    public sealed class ConfirmNewGameScreenViewModel : ScreenViewModelBase
    {
        private readonly ScreenStackService _screenStackService;
        private Action _onConfirmed;

        public ConfirmNewGameScreenViewModel(ScreenStackService screenStackService)
        {
            _screenStackService = screenStackService;
        }

        public override ScreenId Id => ScreenId.ConfirmNewGame;

        protected override void OnOpen(object args)
        {
            _onConfirmed = args as Action;
        }

        protected override void OnClose()
        {
            _onConfirmed = null;
        }

        public void Confirm()
        {
            _screenStackService.CloseTop();
            _onConfirmed?.Invoke();
        }

        public void Cancel()
        {
            _screenStackService.CloseTop();
        }
    }
}
