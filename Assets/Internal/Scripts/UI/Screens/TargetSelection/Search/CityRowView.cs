using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public class CityRowView : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private CityInfoView _cityInfo;

        [Header("Action")]
        [SerializeField] private Button _selectButton;
        [SerializeField] private TextMeshProUGUI _selectButtonText;
        [SerializeField] private LocalizedString _selectButtonLocalized;

        private Action _onSelectClicked;
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _selectButtonHandle;

        public void SetTooltipService(TooltipService service)
        {
            if (_cityInfo != null)
                _cityInfo.SetTooltipService(service);
        }

        public void SetLocalization(LocalizationService localization)
        {
            _localization = localization;
            if (_cityInfo != null)
                _cityInfo.SetLocalization(localization);
            BindSelectButton();
        }

        public void Initialize(CityRowData data, Action onSelectClicked)
        {
            _onSelectClicked = onSelectClicked;

            if (_cityInfo != null)
                _cityInfo.Apply(data.CityIcon, data.Name, data.CityTooltip,
                    data.BuildingEntries, data.QuestIndicatorText);

            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(HandleSelectClick);
            }
        }

        private void BindSelectButton()
        {
            _selectButtonHandle?.Dispose();
            if (_selectButtonText == null || _selectButtonLocalized == null || _localization == null)
                return;
            _selectButtonHandle = _localization.BindText(
                _selectButtonText, _selectButtonLocalized, "CityRow.Select");
        }

        private void HandleSelectClick()
        {
            _onSelectClicked?.Invoke();
        }

        private void OnDestroy()
        {
            _selectButtonHandle?.Dispose();
            _selectButtonHandle = null;
            if (_selectButton != null)
                _selectButton.onClick.RemoveAllListeners();
        }
    }
}
