using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    public class LanguageView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private FillBar _slider;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        private LocalizationService.LocalizedTextHandle _nameHandle;
        private LocalizationService.LocalizedTextHandle _descHandle;
        private LocalizationService.LocalizedTextHandle _valueHandle;

        public void Initialize(LocalizationService localization, LocalizedString name, LocalizedString description,
            float progress, LocalizedString value)
        {
            if (_nameText != null && name != null && localization != null)
                _nameHandle = localization.BindText(_nameText, name, "LanguageView.Name");

            if (_descriptionText != null && description != null && localization != null)
                _descHandle = localization.BindText(_descriptionText, description, "LanguageView.Description");

            if (_slider != null)
                _slider.SetFill(progress);

            if (_valueText != null && value != null && localization != null)
                _valueHandle = localization.BindText(_valueText, value, "LanguageView.Value");
        }

        private void OnDestroy()
        {
            _nameHandle?.Dispose();
            _descHandle?.Dispose();
            _valueHandle?.Dispose();
        }
    }
}
