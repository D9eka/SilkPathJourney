using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;

namespace Internal.Scripts.UI.Screens.TargetSelection
{
    public sealed class TargetSelectionScreenViewModel : ScreenViewModelBase
    {
        private readonly IPlayerStartMovement _playerStartMovement;
        private readonly ScreenStackService _screenStackService;

        public TargetSelectionScreenViewModel(
            IPlayerStartMovement playerStartMovement,
            ScreenStackService screenStackService)
        {
            _playerStartMovement = playerStartMovement;
            _screenStackService = screenStackService;
        }

        public override ScreenId Id => ScreenId.TargetSelection;

        protected override void OnOpen(object args)
        {
            _playerStartMovement.OnSelectionStateChanged += HandleSelectionStateChanged;
            _playerStartMovement.BeginSelection();
        }

        protected override void OnClose()
        {
            _playerStartMovement.OnSelectionStateChanged -= HandleSelectionStateChanged;
            if (_playerStartMovement.IsChoosingTarget)
                _playerStartMovement.CancelSelection();
        }

        public void Cancel()
        {
            _screenStackService.Close(ScreenId.TargetSelection);
        }

        private void HandleSelectionStateChanged(bool isSelecting)
        {
            if (!isSelecting)
                _screenStackService.Close(ScreenId.TargetSelection);
        }
    }
}
