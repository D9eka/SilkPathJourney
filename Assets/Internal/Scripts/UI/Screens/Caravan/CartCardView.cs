using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Caravan
{
    public class CartCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private TextMeshProUGUI _effectAndCompanionText;
        [SerializeField] private TextMeshProUGUI _consumptionText;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private Button _repairButton;
        [SerializeField] private TextMeshProUGUI _repairButtonText;
        [SerializeField] private LocalizedString _repairLocalizedString;
        [SerializeField] private Button _discardButton;
        [SerializeField] private TextMeshProUGUI _discardButtonText;
        [SerializeField] private LocalizedString _discardLocalizedString;
        [SerializeField] private GameObject _buttonsContainer;

        private int _index;
        private Action<int> _onRepair;
        private Action<int> _onDiscard;

        public void Initialize(CartViewData data, Action<int> onRepair, Action<int> onDiscard)
        {
            _index = data.Index;
            _onRepair = onRepair;
            _onDiscard = onDiscard;

            if (_typeText != null)
                _typeText.text = data.TypeName;

            if (_effectAndCompanionText != null)
                _effectAndCompanionText.text = data.EffectAndCompanionText;

            if (_consumptionText != null)
                _consumptionText.text = data.ConsumptionText;

            if (_durabilityText != null)
                _durabilityText.text = data.DurabilityText;

            bool anyButtons = data.CanRepair || data.CanDiscard;
            _buttonsContainer?.SetActive(anyButtons);

            if (_repairButton != null)
            {
                _repairButton.gameObject.SetActive(data.CanRepair);
                _repairButton.onClick.AddListener(HandleRepairClick);
            }

            if (_repairButtonText != null)
                _repairButtonText.text = _repairLocalizedString != null
                    ? LocalizationService.ResolveString(_repairLocalizedString, "UI.Caravan.Cart.Button.Repair", "CartCard.Repair")
                    : "UI.Caravan.Cart.Button.Repair";

            if (_discardButton != null)
            {
                _discardButton.gameObject.SetActive(data.CanDiscard);
                _discardButton.onClick.AddListener(HandleDiscardClick);
            }

            if (_discardButtonText != null)
                _discardButtonText.text = _discardLocalizedString != null
                    ? LocalizationService.ResolveString(_discardLocalizedString, "UI.Caravan.Cart.Button.Discard", "CartCard.Discard")
                    : "UI.Caravan.Cart.Button.Discard";
        }

        private void HandleRepairClick()
        {
            _onRepair?.Invoke(_index);
        }

        private void HandleDiscardClick()
        {
            _onDiscard?.Invoke(_index);
        }

        private void OnDestroy()
        {
            if (_repairButton != null)
                _repairButton.onClick.RemoveListener(HandleRepairClick);

            if (_discardButton != null)
                _discardButton.onClick.RemoveListener(HandleDiscardClick);
        }
    }
}
