using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Pause
{
    public class ConfirmEndRunScreen : PopupScreen
    {
        [Header("Confirm Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        [Header("Confirm Button Texts")]
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        [Header("Body")]
        [SerializeField] private TextMeshProUGUI _bodyText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _bodyLocalizedString;
        [SerializeField] private LocalizedString _confirmLocalizedString;
        [SerializeField] private LocalizedString _cancelLocalizedString;

        private ConfirmEndRunScreenViewModel _viewModel;
        private LocalizationService.LocalizedTextGroup _handles;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as ConfirmEndRunScreenViewModel;
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindLocalization();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _confirmButton.onClick.AddListener(OnConfirm);
            _cancelButton.onClick.AddListener(OnCancel);
            if (Localization != null)
                BindLocalization();
        }

        protected override void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirm);
            _cancelButton.onClick.RemoveListener(OnCancel);
            _handles?.Dispose();
            _handles = null;
            base.OnDisable();
        }

        private void OnConfirm() => _viewModel?.Confirm();
        private void OnCancel() => _viewModel?.Cancel();

        private void BindLocalization()
        {
            _handles?.Dispose();
            _handles = Localization.CreateTextGroup();
            _handles.Bind(_bodyText, _bodyLocalizedString, "ConfirmEndRun.Body");
            _handles.Bind(_confirmButtonText, _confirmLocalizedString, "ConfirmEndRun.Confirm");
            _handles.Bind(_cancelButtonText, _cancelLocalizedString, "ConfirmEndRun.Cancel");
        }
    }
}
