using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Pause
{
    public class PauseScreen : ScreenViewBase
    {
        [SerializeField] private Button _backToGameButton;
        [SerializeField] private Button _exitToMenuButton;
        [SerializeField] private Button _quitButton;

        private PauseScreenViewModel _viewModel;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as PauseScreenViewModel;
        }

        private void OnEnable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.AddListener(OnBackToGame);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.AddListener(OnExitToMenu);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuit);
        }

        private void OnDisable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.RemoveListener(OnBackToGame);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.RemoveListener(OnExitToMenu);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
        }

        private void OnBackToGame() => RaiseCloseRequested();
        private void OnExitToMenu() => _viewModel?.ExitToMenu();
        private void OnQuit() => _viewModel?.QuitGame();
    }
}
