using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Components
{
    public class LabeledBarView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private FillBar _slider;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        private LocalizationService.LocalizedTextHandle _nameHandle;
        private LocalizationService.LocalizedTextHandle _descHandle;
        private LocalizationService.LocalizedTextHandle _valueHandle;

        public void Initialize(LocalizationService loc, LocalizedString name, LocalizedString description,
            float progress, string value)
        {
            ApplyCommon(loc, name, description, progress);
            if (_valueText != null)
                _valueText.text = value;
        }

        public void Initialize(LocalizationService loc, LocalizedString name, LocalizedString description,
            float progress, LocalizedString value)
        {
            ApplyCommon(loc, name, description, progress);
            if (_valueText != null && value != null && loc != null)
                _valueHandle = loc.BindText(_valueText, value, "LabeledBarView.Value");
        }

        private void ApplyCommon(LocalizationService loc, LocalizedString name, LocalizedString description,
            float progress)
        {
            DisposeHandles();

            if (_nameText != null && name != null && loc != null)
                _nameHandle = loc.BindText(_nameText, name, "LabeledBarView.Name");

            if (_descriptionText != null && description != null && loc != null)
                _descHandle = loc.BindText(_descriptionText, description, "LabeledBarView.Description");

            if (_slider != null)
                _slider.SetFill(progress);
        }

        private void DisposeHandles()
        {
            _nameHandle?.Dispose();
            _descHandle?.Dispose();
            _valueHandle?.Dispose();
            _nameHandle = null;
            _descHandle = null;
            _valueHandle = null;
        }

        private void OnDestroy()
        {
            DisposeHandles();
        }
    }
}
