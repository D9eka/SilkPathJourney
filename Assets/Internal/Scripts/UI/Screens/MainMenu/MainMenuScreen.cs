using System;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using R3;
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
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _legacyShopButton;

        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI _newGameButtonText;
        [SerializeField] private TextMeshProUGUI _continueButtonText;
        [SerializeField] private TextMeshProUGUI _profileButtonText;
        [SerializeField] private TextMeshProUGUI _settingsButtonText;
        [SerializeField] private TextMeshProUGUI _quitButtonText;
        [SerializeField] private TextMeshProUGUI _legacyShopButtonText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _newGameLocalizedString;
        [SerializeField] private LocalizedString _continueLocalizedString;
        [SerializeField] private LocalizedString _profileLocalizedString;
        [SerializeField] private LocalizedString _settingsLocalizedString;
        [SerializeField] private LocalizedString _quitLocalizedString;
        [SerializeField] private LocalizedString _legacyShopLocalizedString;

        private MainMenuScreenViewModel _viewModel;
        private LocalizationService.LocalizedTextHandle _headerHandle;
        private LocalizationService.LocalizedTextGroup _buttonHandles;
        private IDisposable _hasRunSubscription;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as MainMenuScreenViewModel;
            SubscribeViewModel();
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
            if (_profileButton != null)
                _profileButton.onClick.AddListener(OnProfile);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettings);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuit);
            if (_legacyShopButton != null)
                _legacyShopButton.onClick.AddListener(OnLegacyShop);
            if (Localization != null)
            {
                BindHeaderLocalization();
                BindButtonLocalization();
            }
            SubscribeViewModel();
        }

        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(OnNewGame);
            _continueButton.onClick.RemoveListener(OnContinue);
            if (_profileButton != null)
                _profileButton.onClick.RemoveListener(OnProfile);
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettings);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
            if (_legacyShopButton != null)
                _legacyShopButton.onClick.RemoveListener(OnLegacyShop);
            _headerHandle?.Dispose();
            _headerHandle = null;
            _buttonHandles?.Dispose();
            _buttonHandles = null;
            _hasRunSubscription?.Dispose();
            _hasRunSubscription = null;
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _hasRunSubscription != null)
                return;

            _hasRunSubscription = _viewModel.HasActiveRun.Subscribe(hasRun =>
            {
                _continueButton.gameObject.SetActive(hasRun);
            });
        }

        private void OnNewGame() => _viewModel?.NewGame();
        private void OnContinue() => _viewModel?.Continue();
        private void OnProfile() => _viewModel?.OnProfile();
        private void OnSettings() { }
        private void OnQuit() => _viewModel?.QuitGame();
        private void OnLegacyShop() => _viewModel?.OpenLegacyShop();

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
            _buttonHandles.Bind(_profileButtonText, _profileLocalizedString, "MainMenu.Profile");
            _buttonHandles.Bind(_settingsButtonText, _settingsLocalizedString, "MainMenu.Settings");
            _buttonHandles.Bind(_quitButtonText, _quitLocalizedString, "MainMenu.Quit");
            _buttonHandles.Bind(_legacyShopButtonText, _legacyShopLocalizedString, "MainMenu.LegacyShop");
        }
    }
}
