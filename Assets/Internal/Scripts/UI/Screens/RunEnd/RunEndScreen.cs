using System;
using Internal.Scripts.Meta;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.RunEnd.Stats;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.RunEnd
{
    public sealed class RunEndScreen : ScreenViewBase
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [Header("Stats")]
        [SerializeField] private PathStatsView _pathStats;
        [SerializeField] private EconomyStatsView _economyStats;
        [SerializeField] private CaravanStatsView _caravanStats;
        [SerializeField] private AdventuresStatsView _adventuresStats;
        [SerializeField] private LegacyStatsView _legacyStats;
        [SerializeField] private ResourceIndicator _legacyPointsIndicator;

        [Header("Buttons")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TextMeshProUGUI _mainMenuButtonText;
        [SerializeField] private Button _newRunButton;
        [SerializeField] private TextMeshProUGUI _newRunButtonText;
        [SerializeField] private Button _legacyShopButton;
        [SerializeField] private TextMeshProUGUI _legacyShopButtonText;

        private RunEndScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private LocalizationService.LocalizedTextHandle _titleHandle;
        private LocalizationService.LocalizedTextHandle _subtitleHandle;
        private LocalizationService.LocalizedTextHandle _mainMenuButtonHandle;
        private LocalizationService.LocalizedTextHandle _newRunButtonHandle;
        private LocalizationService.LocalizedTextHandle _legacyShopButtonHandle;

        private void OnEnable()
        {
            SubscribeViewModel();
        }

        protected override void OnLocalizationReady()
        {
            _pathStats.BindLocalization(Localization);
            _economyStats.BindLocalization(Localization);
            _caravanStats.BindLocalization(Localization);
            _adventuresStats.BindLocalization(Localization);

            _mainMenuButtonHandle?.Dispose();
            _mainMenuButtonHandle = Localization.BindText(_mainMenuButtonText, "UI", LocUI.UI_RunEnd_Button_MainMenu, $"{name}.MainMenuButton");
            _newRunButtonHandle?.Dispose();
            _newRunButtonHandle = Localization.BindText(_newRunButtonText, "UI", LocUI.UI_RunEnd_Button_NewRun, $"{name}.NewRunButton");
            _legacyShopButtonHandle?.Dispose();
            _legacyShopButtonHandle = Localization.BindText(_legacyShopButtonText, "UI", LocUI.UI_RunEnd_Button_LegacyShop, $"{name}.LegacyShopButton");
        }

        private void OnDisable()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _titleHandle?.Dispose();
            _titleHandle = null;
            _subtitleHandle?.Dispose();
            _subtitleHandle = null;
            _pathStats.DisposeBinding();
            _economyStats.DisposeBinding();
            _caravanStats.DisposeBinding();
            _adventuresStats.DisposeBinding();
            _mainMenuButtonHandle?.Dispose();
            _mainMenuButtonHandle = null;
            _newRunButtonHandle?.Dispose();
            _newRunButtonHandle = null;
            _legacyShopButtonHandle?.Dispose();
            _legacyShopButtonHandle = null;

            _mainMenuButton.onClick.RemoveAllListeners();
            _newRunButton.onClick.RemoveAllListeners();
            _legacyShopButton.onClick.RemoveAllListeners();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as RunEndScreenViewModel;
            SetupIcons();
            SubscribeViewModel();
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _legacyPointsIndicator?.SetIcon(icons.Get(ResourceType.LegacyPoints)?.Icon);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);

            _mainMenuButton.onClick.AddListener(() => _viewModel.OpenMainMenu());
            _newRunButton.onClick.AddListener(() => _viewModel.OpenNewGame());
            _legacyShopButton.onClick.AddListener(() => _viewModel.OpenLegacyShop());
        }

        private void ApplyState(RunEndViewState state)
        {
            if (state == null) return;

            BindTitleForEndType(state.EndType);
            BindSubtitle(state.ReasonKey);

            _pathStats.Apply(state.Stats);
            _economyStats.Apply(state.Stats, state.BestDealName);
            _caravanStats.Apply(state.Stats);
            _adventuresStats.Apply(state.Stats, state.LanguagesNames);
            _legacyStats.Apply(state.LegacyPointsEarned, state.LegacyPointsBonus);
            _legacyPointsIndicator?.SetValue(state.LegacyPointsTotal);
        }

        private void BindTitleForEndType(EndType endType)
        {
            _titleHandle?.Dispose();
            if (Localization == null) return;

            string key = endType switch
            {
                EndType.Victory => LocUI.UI_RunEnd_Title_Victory,
                EndType.Defeat_Bankruptcy => LocUI.UI_RunEnd_Title_Defeat_Bankruptcy,
                EndType.Defeat_Mutiny => LocUI.UI_RunEnd_Title_Defeat_Mutiny,
                EndType.Defeat_CaravanLost => LocUI.UI_RunEnd_Title_Defeat_CaravanLost,
                EndType.Defeat_Famine => LocUI.UI_RunEnd_Title_Defeat_Famine,
                _ => LocUI.UI_RunEnd_Title_Defeat_Bankruptcy
            };
            _titleHandle = Localization.BindText(_titleText, "UI", key, $"{name}.Title");
        }

        private void BindSubtitle(string reasonKey)
        {
            _subtitleHandle?.Dispose();
            if (Localization == null || _subtitleText == null || string.IsNullOrEmpty(reasonKey)) return;
            _subtitleHandle = Localization.BindText(_subtitleText, "UI", reasonKey, $"{name}.Subtitle");
        }
    }
}
