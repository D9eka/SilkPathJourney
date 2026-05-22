using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Pause
{
    public class PauseScreen : ScreenViewBase
    {
        [Header("Header")]
        [SerializeField] private HeaderElement _header;

        [Header("Buttons")]
        [SerializeField] private Button _backToGameButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitToMenuButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _endRunButton;

        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI _backToGameButtonText;
        [SerializeField] private TextMeshProUGUI _settingsButtonText;
        [SerializeField] private TextMeshProUGUI _exitToMenuButtonText;
        [SerializeField] private TextMeshProUGUI _quitButtonText;
        [SerializeField] private TextMeshProUGUI _endRunButtonText;

        [Header("Auto Save Hint")]
        [SerializeField] private TextMeshProUGUI _autoSaveHintText;
        [SerializeField] private LocalizedString _autoSaveHintLocalizedString;

        [Header("Survey")]
        [SerializeField] private SurveyBlock _surveyBlock;

        [Header("Localization")]
        [SerializeField] private LocalizedString _headerLocalizedString;
        [SerializeField] private LocalizedString _backToGameLocalizedString;
        [SerializeField] private LocalizedString _settingsLocalizedString;
        [SerializeField] private LocalizedString _exitToMenuLocalizedString;
        [SerializeField] private LocalizedString _quitLocalizedString;
        [SerializeField] private LocalizedString _endRunLocalizedString;

        private PauseScreenViewModel _viewModel;
        private LocalizationService.LocalizedTextHandle _headerHandle;
        private LocalizationService.LocalizedTextHandle _autoSaveHintHandle;
        private LocalizationService.LocalizedTextGroup _buttonHandles;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as PauseScreenViewModel;
        }

        protected override void OnLocalizationReady()
        {
            BindHeaderLocalization();
            BindButtonLocalization();
            BindAutoSaveHintLocalization();
            _surveyBlock.BindLocalization(Localization);
        }

        private void OnEnable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.AddListener(OnBackToGame);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettings);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.AddListener(OnExitToMenu);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuit);
            if (_endRunButton != null)
                _endRunButton.onClick.AddListener(OnEndRun);
            if (Localization != null)
            {
                BindHeaderLocalization();
                BindButtonLocalization();
                BindAutoSaveHintLocalization();
            }
        }

        private void OnDisable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.RemoveListener(OnBackToGame);
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettings);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.RemoveListener(OnExitToMenu);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
            if (_endRunButton != null)
                _endRunButton.onClick.RemoveListener(OnEndRun);
            _headerHandle?.Dispose();
            _headerHandle = null;
            _autoSaveHintHandle?.Dispose();
            _autoSaveHintHandle = null;
            _buttonHandles?.Dispose();
            _buttonHandles = null;
        }

        private void OnBackToGame() => RaiseCloseRequested();
        private void OnSettings() { }
        private void OnExitToMenu() => _viewModel?.ExitToMenu();
        private void OnQuit() => _viewModel?.QuitGame();
        private void OnEndRun() => _viewModel?.EndRun();

        private void BindHeaderLocalization()
        {
            _headerHandle?.Dispose();
            if (_header != null && _header.Text != null && _headerLocalizedString != null)
                _headerHandle = Localization.BindText(_header.Text, _headerLocalizedString, "Pause.Header");
        }

        private void BindAutoSaveHintLocalization()
        {
            _autoSaveHintHandle?.Dispose();
            if (_autoSaveHintText != null && _autoSaveHintLocalizedString != null)
                _autoSaveHintHandle = Localization.BindText(
                    _autoSaveHintText, _autoSaveHintLocalizedString, "Pause.AutoSaveHint");
        }

        private void BindButtonLocalization()
        {
            _buttonHandles?.Dispose();
            _buttonHandles = Localization.CreateTextGroup();
            _buttonHandles.Bind(_backToGameButtonText, _backToGameLocalizedString, "Pause.BackToGame");
            _buttonHandles.Bind(_settingsButtonText, _settingsLocalizedString, "Pause.Settings");
            _buttonHandles.Bind(_exitToMenuButtonText, _exitToMenuLocalizedString, "Pause.ExitToMenu");
            _buttonHandles.Bind(_quitButtonText, _quitLocalizedString, "Pause.Quit");
            _buttonHandles.Bind(_endRunButtonText, _endRunLocalizedString, "Pause.EndRun");
        }
    }
}
