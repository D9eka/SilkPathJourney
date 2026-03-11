using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.TargetSelection
{
    public class TargetSelectionScreen : ScreenViewBase
    {
        [SerializeField] protected CloseButton _cancelButton;
        [Header("Confirmation")]
        [SerializeField] private GameObject _confirmContainer;
        [SerializeField] private TextMeshProUGUI _headerText;
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

            SetStaticTexts();
        }

        public override void Show()
        {
            ResetPreviewState();
            base.Show();
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

        private void SetStaticTexts()
        {
            if (_confirmButtonText != null && _confirmLocalizedString != null)
                _confirmButtonText.text = LocalizationService.ResolveString(
                    _confirmLocalizedString, "Yes", "UI.TargetSelection.Main.Button.Confirm");
            if (_cancelPreviewButtonText != null && _cancelLocalizedString != null)
                _cancelPreviewButtonText.text = LocalizationService.ResolveString(
                    _cancelLocalizedString, "No", "UI.TargetSelection.Main.Button.Cancel");
        }

        private void ApplyPreview(CityData city, TravelEstimate estimate)
        {
            bool inPreview = city != null;

            if (_confirmContainer != null)
                _confirmContainer.SetActive(inPreview);
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(!inPreview);

            if (inPreview && _headerText != null)
            {
                string cityName = city.Name.GetLocalizedString();
                _headerText.text = LocalizationService.ResolveString(
                    _travelToLocalizedString,
                    $"Travel to {cityName}?",
                    "UI.TargetSelection.Main.Label.TravelTo",
                    cityName);
            }

            if (_travelInfoText != null)
            {
                if (inPreview && estimate.IsValid)
                {
                    string daysLine = LocalizationService.ResolveString(
                        _travelDaysLocalizedString,
                        $"{estimate.Days} days at normal speed",
                        "UI.TargetSelection.Main.Label.TravelDays",
                        estimate.Days);

                    LocalizedString suppliesString = estimate.SuppliesSufficient
                        ? _suppliesSufficientLocalizedString
                        : _suppliesInsufficientLocalizedString;
                    string suppliesLine = LocalizationService.ResolveString(
                        suppliesString,
                        estimate.SuppliesSufficient ? "Supplies sufficient" : "Supplies insufficient",
                        estimate.SuppliesSufficient
                            ? "UI.TargetSelection.Main.Label.SuppliesSufficient"
                            : "UI.TargetSelection.Main.Label.SuppliesInsufficient");

                    _travelInfoText.text = $"{daysLine}\n{suppliesLine}";
                    _travelInfoText.gameObject.SetActive(true);
                }
                else
                {
                    _travelInfoText.gameObject.SetActive(false);
                }
            }
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

            base.OnDestroy();
        }

        private void OnConfirm() => _viewModel?.ConfirmTarget();
        private void OnCancelPreview() => _viewModel?.CancelPreview();
    }
}
