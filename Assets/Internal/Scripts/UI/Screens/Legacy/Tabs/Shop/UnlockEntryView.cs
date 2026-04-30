using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Shop
{
    public sealed class UnlockEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;

        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _buttonHandle;
        private Action _onUnlock;

        private LocalizedString _buyLocalized;
        private LocalizedString _boughtLocalized;
        private LocalizedString _notEnoughLocalized;

        private void Awake()
        {
            _buyLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Unlock_Buy);
            _boughtLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Unlock_Bought);
            _notEnoughLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Unlock_NotEnough);
        }

        public void BindLocalization(LocalizationService localization)
        {
            _localization = localization;
        }

        private void OnDestroy()
        {
            _buttonHandle?.Dispose();
        }

        public void Apply(UnlockEntryData data, int currentPoints, Action onUnlock)
        {
            _onUnlock = onUnlock;

            _nameText.text = data.Name;
            _descriptionText.text = data.Description;

            if (_buyButton == null) return;

            _buyButton.onClick.RemoveAllListeners();

            if (data.IsUnlocked)
            {
                BindButtonText(_boughtLocalized, null);
                _buyButton.interactable = false;
            }
            else if (currentPoints < data.Cost)
            {
                BindButtonText(_notEnoughLocalized, null);
                _buyButton.interactable = false;
            }
            else
            {
                BindButtonText(_buyLocalized, data.Cost);
                _buyButton.interactable = true;
                _buyButton.onClick.AddListener(OnBuy);
            }
        }

        private void BindButtonText(LocalizedString localized, object arg)
        {
            _buttonHandle?.Dispose();
            if (_localization == null) return;
            _buttonHandle = arg != null
                ? _localization.BindText(_buyButtonText, localized, $"{name}.BuyButton", string.Empty, null, arg)
                : _localization.BindText(_buyButtonText, localized, $"{name}.BuyButton");
        }

        private void OnBuy() => _onUnlock?.Invoke();
    }
}
