using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.MainMenu
{
    public class MainMenuScreen : ScreenViewBase
    {
        [Header("Header")]
        [SerializeField] private HeaderElement _header;
        [SerializeField] private LocalizedString _headerLocalizedString;

        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI _newGameButtonText;
        [SerializeField] private TextMeshProUGUI _continueButtonText;
        [SerializeField] private TextMeshProUGUI _loadButtonText;
        [SerializeField] private TextMeshProUGUI _settingsButtonText;
        [SerializeField] private TextMeshProUGUI _quitButtonText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _newGameLocalizedString;
        [SerializeField] private LocalizedString _continueLocalizedString;
        [SerializeField] private LocalizedString _loadLocalizedString;
        [SerializeField] private LocalizedString _settingsLocalizedString;
        [SerializeField] private LocalizedString _quitLocalizedString;

        private MainMenuScreenViewModel _viewModel;
        private LocalizationService.LocalizedTextHandle _headerHandle;
        private LocalizationService.LocalizedTextGroup _buttonHandles;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as MainMenuScreenViewModel;
            bool hasSave = _viewModel != null && _viewModel.HasSave;
            _continueButton.interactable = hasSave;
            if (_loadButton != null)
                _loadButton.interactable = hasSave;
        }

        protected override void OnLocalizationReady()
        {
            BindHeaderLocalization();
            BindButtonLocalization();
        }

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            if (_loadButton != null)
                _loadButton.onClick.AddListener(OnLoad);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettings);
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
            _newGameButton.onClick.RemoveListener(OnNewGame);
            _continueButton.onClick.RemoveListener(OnContinue);
            if (_loadButton != null)
                _loadButton.onClick.RemoveListener(OnLoad);
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettings);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
            _headerHandle?.Dispose();
            _headerHandle = null;
            _buttonHandles?.Dispose();
            _buttonHandles = null;
        }

        private void OnNewGame() => _viewModel?.NewGame();
        private void OnContinue() => _viewModel?.Continue();
        private void OnLoad() => _viewModel?.OpenLoad();
        private void OnSettings() { }
        private void OnQuit() => _viewModel?.QuitGame();

        private void BindHeaderLocalization()
        {
            _headerHandle?.Dispose();
            if (_header != null && _header.Text != null && _headerLocalizedString != null)
                _headerHandle = Localization.BindText(_header.Text, _headerLocalizedString, "MainMenu.Header");
        }

        private void BindButtonLocalization()
        {
            _buttonHandles?.Dispose();
            _buttonHandles = Localization.CreateTextGroup();
            _buttonHandles.Bind(_newGameButtonText, _newGameLocalizedString, "MainMenu.NewGame");
            _buttonHandles.Bind(_continueButtonText, _continueLocalizedString, "MainMenu.Continue");
            _buttonHandles.Bind(_loadButtonText, _loadLocalizedString, "MainMenu.Load");
            _buttonHandles.Bind(_settingsButtonText, _settingsLocalizedString, "MainMenu.Settings");
            _buttonHandles.Bind(_quitButtonText, _quitLocalizedString, "MainMenu.Quit");
        }
    }
}
