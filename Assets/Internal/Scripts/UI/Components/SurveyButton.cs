using Internal.Scripts.UI.Localization;
using Internal.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    public class SurveyButton : MonoBehaviour
    {
        [SerializeField] private Button _surveyButton;
        [SerializeField] private TextMeshProUGUI _surveyButtonText;
        [SerializeField] private LocalizedString _surveyButtonLocalizedString;

        protected LocalizationService Localization { get; private set; }
        private LocalizationService.LocalizedTextHandle _buttonHandle;

        protected virtual void OnEnable()
        {
            _surveyButton.onClick.AddListener(OnSurvey);
            if (Localization != null)
                BindLocalization(Localization);
        }

        protected virtual void OnDisable()
        {
            _surveyButton.onClick.RemoveListener(OnSurvey);
            _buttonHandle?.Dispose();
            _buttonHandle = null;
        }

        public virtual void BindLocalization(LocalizationService localization)
        {
            Localization = localization;
            _buttonHandle?.Dispose();
            _buttonHandle = localization.BindText(_surveyButtonText, _surveyButtonLocalizedString, $"{name}.SurveyButton");
        }

        private void OnSurvey() => SurveyLink.Open();
    }
}
