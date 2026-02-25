using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.MainMenu
{
    public class MainMenuScreen : ScreenViewBase
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _quitButton;

        private MainMenuScreenViewModel _viewModel;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as MainMenuScreenViewModel;
            _continueButton.interactable = _viewModel != null && _viewModel.HasSave;
        }

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuit);
        }

        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(OnNewGame);
            _continueButton.onClick.RemoveListener(OnContinue);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
        }

        private void OnNewGame() => _viewModel?.NewGame();
        private void OnContinue() => _viewModel?.Continue();
        private void OnQuit() => _viewModel?.QuitGame();
    }
}
