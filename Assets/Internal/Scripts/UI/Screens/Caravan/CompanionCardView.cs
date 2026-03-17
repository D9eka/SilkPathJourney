using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Caravan
{
    public enum CompanionCardAction
    {
        Heal,
        Fire
    }

    public class CompanionCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _typeAndNameText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private TextMeshProUGUI _dailyCostText;
        [SerializeField] private Button _healButton;
        [SerializeField] private TextMeshProUGUI _healButtonText;
        [SerializeField] private LocalizedString _healLocalizedString;
        [SerializeField] private Button _fireButton;
        [SerializeField] private TextMeshProUGUI _fireButtonText;
        [SerializeField] private LocalizedString _fireLocalizedString;

        private int _index;
        private Action<int, CompanionCardAction> _callback;

        public void Initialize(CompanionCardData data, Action<int, CompanionCardAction> callback)
        {
            _index = data.Index;
            _callback = callback;

            if (_typeAndNameText != null)
                _typeAndNameText.text = data.TypeAndName;

            if (_statusText != null)
                _statusText.text = data.StatusText;

            if (_effectText != null)
                _effectText.text = data.EffectText;

            if (_dailyCostText != null)
                _dailyCostText.text = data.DailyCostText;

            if (_healButton != null)
            {
                _healButton.gameObject.SetActive(data.IsInjured && data.CanHeal);
                _healButton.onClick.AddListener(HandleHealClick);
            }

            if (_healButtonText != null)
                _healButtonText.text = _healLocalizedString != null
                    ? LocalizationService.ResolveString(_healLocalizedString, "UI.Caravan.Companion.Button.Heal", "CompanionCard.Heal")
                    : "UI.Caravan.Companion.Button.Heal";

            if (_fireButton != null)
                _fireButton.onClick.AddListener(HandleFireClick);

            if (_fireButtonText != null)
                _fireButtonText.text = _fireLocalizedString != null
                    ? LocalizationService.ResolveString(_fireLocalizedString, "UI.Caravan.Companion.Button.Fire", "CompanionCard.Fire")
                    : "UI.Caravan.Companion.Button.Fire";
        }

        private void HandleHealClick()
        {
            _callback?.Invoke(_index, CompanionCardAction.Heal);
        }

        private void HandleFireClick()
        {
            _callback?.Invoke(_index, CompanionCardAction.Fire);
        }

        private void OnDestroy()
        {
            if (_healButton != null)
                _healButton.onClick.RemoveListener(HandleHealClick);

            if (_fireButton != null)
                _fireButton.onClick.RemoveListener(HandleFireClick);
        }
    }
}
