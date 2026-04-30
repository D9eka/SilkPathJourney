using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.TargetSelection.Search;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.TargetSelection
{
    public class TargetSelectionScreen : ScreenViewBase
    {
        [SerializeField] protected CloseButton _cancelButton;
        [Header("Search Panel")]
        [SerializeField] private CitySearchPanelView _searchPanel;
        [Header("Confirmation")]
        [SerializeField] private GameObject _confirmContainer;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private CityInfoView _confirmCityInfo;
        [SerializeField] private TextMeshProUGUI _travelInfoText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelPreviewButton;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private TextMeshProUGUI _cancelPreviewButtonText;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _travelToLocalizedString;
        [SerializeField] private LocalizedString _confirmLocalizedString;
        [SerializeField] private LocalizedString _cancelLocalizedString;
        [SerializeField] private LocalizedString _travelDaysLocalizedString;
        [SerializeField] private LocalizedString _suppliesSufficientLocalizedString;
        [SerializeField] private LocalizedString _suppliesInsufficientLocalizedString;

        private TargetSelectionScreenViewModel _viewModel;
        private LocalizationService.LocalizedTextHandle _headerHandle;
        private LocalizationService.LocalizedTextHandle _confirmButtonHandle;
        private LocalizationService.LocalizedTextHandle _cancelButtonHandle;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as TargetSelectionScreenViewModel;
            if (_cancelButton != null)
                _cancelButton.Clicked += RaiseCloseRequested;
            _viewModel.PreviewChanged += ApplyPreview;

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirm);
            if (_cancelPreviewButton != null)
                _cancelPreviewButton.onClick.AddListener(OnCancelPreview);

            _searchPanel?.Bind(_viewModel.SearchPanel);
            _searchPanel?.SetLocalization(Localization);
            if (_confirmCityInfo != null)
            {
                _confirmCityInfo.SetTooltipService(_viewModel.SearchPanel?.TooltipService);
                _confirmCityInfo.SetLocalization(Localization);
            }

            BindStaticTexts();
        }

        public override void Show()
        {
            ResetPreviewState();
            base.Show();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            _searchPanel?.SetLocalization(Localization);
            _confirmCityInfo?.SetLocalization(Localization);
            BindStaticTexts();
        }

        private void ResetPreviewState()
        {
            if (_confirmContainer != null)
                _confirmContainer.SetActive(false);
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(true);
            if (_travelInfoText != null)
                _travelInfoText.gameObject.SetActive(false);
        }

        private void BindStaticTexts()
        {
            if (Localization == null) return;

            DisposeStaticHandles();

            if (_headerText != null && _travelToLocalizedString != null)
                _headerHandle = Localization.BindText(
                    _headerText, _travelToLocalizedString, "TargetSelection.TravelTo");

            if (_confirmButtonText != null && _confirmLocalizedString != null)
                _confirmButtonHandle = Localization.BindText(
                    _confirmButtonText, _confirmLocalizedString, "TargetSelection.Confirm");

            if (_cancelPreviewButtonText != null && _cancelLocalizedString != null)
                _cancelButtonHandle = Localization.BindText(
                    _cancelPreviewButtonText, _cancelLocalizedString, "TargetSelection.Cancel");
        }

        private void DisposeStaticHandles()
        {
            _headerHandle?.Dispose();
            _headerHandle = null;
            _confirmButtonHandle?.Dispose();
            _confirmButtonHandle = null;
            _cancelButtonHandle?.Dispose();
            _cancelButtonHandle = null;
        }

        private void ApplyPreview(CityData city, TravelEstimate estimate, CityRowData rowData)
        {
            bool inPreview = city != null;

            if (_confirmContainer != null)
                _confirmContainer.SetActive(inPreview);
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(!inPreview);

            if (inPreview && _confirmCityInfo != null)
            {
                _confirmCityInfo.Apply(rowData.CityIcon, rowData.Name, rowData.CityTooltip,
                    rowData.BuildingEntries, rowData.QuestIndicatorText);
            }

            ApplyTravelInfo(inPreview, estimate);
        }

        private void ApplyTravelInfo(bool inPreview, TravelEstimate estimate)
        {
            bool show = inPreview && estimate.IsValid;
            if (_travelInfoText != null)
                _travelInfoText.gameObject.SetActive(show);
            if (!show || _travelInfoText == null) return;

            string days = LocalizationService.ResolveString(_travelDaysLocalizedString,
                estimate.Days.ToString(), "TargetSelection.TravelDays", estimate.Days);
            LocalizedString suppliesStr = estimate.SuppliesSufficient
                ? _suppliesSufficientLocalizedString
                : _suppliesInsufficientLocalizedString;
            string supplies = LocalizationService.ResolveString(suppliesStr, string.Empty,
                estimate.SuppliesSufficient ? "TargetSelection.SuppliesSufficient" : "TargetSelection.SuppliesInsufficient");
            _travelInfoText.text = $"{days}\n{supplies}";
        }

        protected override void OnDestroy()
        {
            if (_viewModel != null)
                _viewModel.PreviewChanged -= ApplyPreview;

            if (_cancelButton != null)
                _cancelButton.Clicked -= RaiseCloseRequested;
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirm);
            if (_cancelPreviewButton != null)
                _cancelPreviewButton.onClick.RemoveListener(OnCancelPreview);

            DisposeStaticHandles();

            base.OnDestroy();
        }

        private void OnConfirm() => _viewModel?.ConfirmTarget();
        private void OnCancelPreview() => _viewModel?.CancelPreview();
    }
}
