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
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitToMenuButton;
        [SerializeField] private Button _quitButton;

        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI _backToGameButtonText;
        [SerializeField] private TextMeshProUGUI _saveButtonText;
        [SerializeField] private TextMeshProUGUI _loadButtonText;
        [SerializeField] private TextMeshProUGUI _settingsButtonText;
        [SerializeField] private TextMeshProUGUI _exitToMenuButtonText;
        [SerializeField] private TextMeshProUGUI _quitButtonText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _headerLocalizedString;
        [SerializeField] private LocalizedString _backToGameLocalizedString;
        [SerializeField] private LocalizedString _saveLocalizedString;
        [SerializeField] private LocalizedString _loadLocalizedString;
        [SerializeField] private LocalizedString _settingsLocalizedString;
        [SerializeField] private LocalizedString _exitToMenuLocalizedString;
        [SerializeField] private LocalizedString _quitLocalizedString;

        private PauseScreenViewModel _viewModel;
        private LocalizationService.LocalizedTextHandle _headerHandle;
        private LocalizationService.LocalizedTextGroup _buttonHandles;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as PauseScreenViewModel;
        }

        protected override void OnLocalizationReady()
        {
            BindHeaderLocalization();
            BindButtonLocalization();
        }

        private void OnEnable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.AddListener(OnBackToGame);
            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSave);
            if (_loadButton != null)
                _loadButton.onClick.AddListener(OnLoad);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettings);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.AddListener(OnExitToMenu);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuit);
            if (Localization != null)
            {
                BindHeaderLocalization();
                BindButtonLocalization();
            }
        }

        private void OnDisable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.RemoveListener(OnBackToGame);
            if (_saveButton != null)
                _saveButton.onClick.RemoveListener(OnSave);
            if (_loadButton != null)
                _loadButton.onClick.RemoveListener(OnLoad);
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettings);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.RemoveListener(OnExitToMenu);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
            _headerHandle?.Dispose();
            _headerHandle = null;
            _buttonHandles?.Dispose();
            _buttonHandles = null;
        }

        private void OnBackToGame() => RaiseCloseRequested();
        private void OnSave() => _viewModel?.OpenSave();
        private void OnLoad() => _viewModel?.OpenLoad();
        private void OnSettings() { }
        private void OnExitToMenu() => _viewModel?.ExitToMenu();
        private void OnQuit() => _viewModel?.QuitGame();

        private void BindHeaderLocalization()
        {
            _headerHandle?.Dispose();
            if (_header != null && _header.Text != null && _headerLocalizedString != null)
                _headerHandle = Localization.BindText(_header.Text, _headerLocalizedString, "Pause.Header");
        }

        private void BindButtonLocalization()
        {
            _buttonHandles?.Dispose();
            _buttonHandles = Localization.CreateTextGroup();
            _buttonHandles.Bind(_backToGameButtonText, _backToGameLocalizedString, "Pause.BackToGame");
            _buttonHandles.Bind(_saveButtonText, _saveLocalizedString, "Pause.Save");
            _buttonHandles.Bind(_loadButtonText, _loadLocalizedString, "Pause.Load");
            _buttonHandles.Bind(_settingsButtonText, _settingsLocalizedString, "Pause.Settings");
            _buttonHandles.Bind(_exitToMenuButtonText, _exitToMenuLocalizedString, "Pause.ExitToMenu");
            _buttonHandles.Bind(_quitButtonText, _quitLocalizedString, "Pause.Quit");
        }
    }
}
