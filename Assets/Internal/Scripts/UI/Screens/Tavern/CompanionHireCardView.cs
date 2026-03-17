using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public class CompanionHireCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _typeAndNameText;
        [SerializeField] private TextMeshProUGUI _qualityText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private TextMeshProUGUI _hireCostText;
        [SerializeField] private TextMeshProUGUI _dailyCostText;
        [SerializeField] private Button _hireButton;
        [SerializeField] private TextMeshProUGUI _hireButtonText;
        [SerializeField] private LocalizedString _hireLocalizedString;

        private int _index;
        private Action<int> _onHire;

        public void Initialize(CompanionHireData data, Action<int> onHire)
        {
            _index = data.Index;
            _onHire = onHire;

            if (_typeAndNameText != null)
                _typeAndNameText.text = data.TypeAndName;

            if (_qualityText != null)
                _qualityText.text = data.QualityName;

            if (_effectText != null)
                _effectText.text = data.EffectText;

            if (_hireCostText != null)
                _hireCostText.text = data.HireCostText;

            if (_dailyCostText != null)
                _dailyCostText.text = data.DailyCostText;

            if (_hireButton != null)
            {
                _hireButton.interactable = data.CanHire;
                _hireButton.onClick.AddListener(HandleHire);
            }

            if (_hireButtonText != null)
                _hireButtonText.text = _hireLocalizedString != null
                    ? LocalizationService.ResolveString(_hireLocalizedString, "UI.Tavern.Companion.Button.Hire", "CompanionHireCard.Hire")
                    : "UI.Tavern.Companion.Button.Hire";
        }

        private void HandleHire()
        {
            _onHire?.Invoke(_index);
        }

        private void OnDestroy()
        {
            if (_hireButton != null)
                _hireButton.onClick.RemoveListener(HandleHire);
        }
    }
}
