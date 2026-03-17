using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Caravan
{
    public class AnimalInfoView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private Button _healButton;
        [SerializeField] private TextMeshProUGUI _healButtonText;
        [SerializeField] private LocalizedString _healLocalizedString;

        private Action _onHealClicked;
        private bool _listenerAdded;

        public void Initialize(AnimalViewData data, Action onHealClicked)
        {
            _onHealClicked = onHealClicked;

            if (_nameText != null)
                _nameText.text = data.TypeName;

            if (_statusText != null)
                _statusText.text = data.StatusText;

            if (_effectText != null)
                _effectText.text = data.EffectText;

            if (_healButton != null)
            {
                _healButton.gameObject.SetActive(data.IsInjured && data.CanHeal);
                if (!_listenerAdded)
                {
                    _healButton.onClick.AddListener(HandleHealClick);
                    _listenerAdded = true;
                }

                if (_healButtonText != null)
                    _healButtonText.text = _healLocalizedString != null
                        ? LocalizationService.ResolveString(_healLocalizedString, "UI.Caravan.Animal.Button.Heal", "AnimalInfo.Heal")
                        : "UI.Caravan.Animal.Button.Heal";
            }
        }

        private void HandleHealClick()
        {
            _onHealClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_healButton != null)
                _healButton.onClick.RemoveListener(HandleHealClick);
        }
    }
}
