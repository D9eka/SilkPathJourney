using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    public sealed class SurveyBlock : SurveyButton
    {
        [SerializeField] private TextMeshProUGUI _surveyHeaderText;
        [SerializeField] private LocalizedString _surveyHeaderLocalizedString;
        [SerializeField] private TextMeshProUGUI _surveyDescriptionText;
        [SerializeField] private LocalizedString _surveyDescriptionLocalizedString;
        [SerializeField] private Image _surveyQrImage;
        [SerializeField] private Sprite _surveyQrRu;
        [SerializeField] private Sprite _surveyQrEn;

        private LocalizationService.LocalizedTextHandle _headerHandle;
        private LocalizationService.LocalizedTextHandle _descriptionHandle;

        protected override void OnEnable()
        {
            base.OnEnable();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            ApplyQr();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _headerHandle?.Dispose();
            _headerHandle = null;
            _descriptionHandle?.Dispose();
            _descriptionHandle = null;
        }

        public override void BindLocalization(LocalizationService localization)
        {
            base.BindLocalization(localization);
            _headerHandle?.Dispose();
            _headerHandle = localization.BindText(_surveyHeaderText, _surveyHeaderLocalizedString, $"{name}.SurveyHeader");
            _descriptionHandle?.Dispose();
            _descriptionHandle = localization.BindText(_surveyDescriptionText, _surveyDescriptionLocalizedString, $"{name}.SurveyDescription");
        }

        private void OnLocaleChanged(Locale _) => ApplyQr();

        private void ApplyQr()
        {
            string code = LocalizationSettings.SelectedLocale?.Identifier.Code;
            _surveyQrImage.sprite = code != null && code.StartsWith("ru") ? _surveyQrRu : _surveyQrEn;
        }
    }
}
