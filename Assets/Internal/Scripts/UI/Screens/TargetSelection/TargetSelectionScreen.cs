using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screen.View;
using Internal.Scripts.UI.Screen.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.TargetSelection
{
    public class TargetSelectionScreen : ScreenViewBase
    {
        [SerializeField] protected CloseButton _cancelButton;

        private TargetSelectionScreenViewModel _viewModel;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as TargetSelectionScreenViewModel;
            _cancelButton.Clicked += RaiseCloseRequested;
        }

        private void OnCancel() => _viewModel?.Cancel();
    }
}
