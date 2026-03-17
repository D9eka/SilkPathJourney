using System;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.UI.Screens.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Workshop
{
    public class ExtraCartCardView : MonoBehaviour
    {
        [SerializeField] private CartStatsView _stats;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TextMeshProUGUI _sellButtonText;

        private ExtraCartType _type;
        private Action<ExtraCartType> _onBuy;
        private Action<ExtraCartType> _onSell;

        public void Initialize(ExtraCartViewData data, string buyLabel, string sellLabel,
            Action<ExtraCartType> onBuy, Action<ExtraCartType> onSell)
        {
            _type = data.Type;
            _onBuy = onBuy;
            _onSell = onSell;

            if (_stats != null)
                _stats.Apply(data.Stats);

            if (_countText != null)
                _countText.text = data.CountText;

            if (_buyButtonText != null)
                _buyButtonText.text = buyLabel;

            if (_buyButton != null)
            {
                _buyButton.interactable = data.CanBuy;
                _buyButton.onClick.RemoveListener(HandleBuy);
                _buyButton.onClick.AddListener(HandleBuy);
            }

            if (_sellButtonText != null)
                _sellButtonText.text = sellLabel;

            if (_sellButton != null)
            {
                _sellButton.interactable = data.CanSell;
                _sellButton.onClick.RemoveListener(HandleSell);
                _sellButton.onClick.AddListener(HandleSell);
            }
        }

        private void HandleBuy()
        {
            _onBuy?.Invoke(_type);
        }

        private void HandleSell()
        {
            _onSell?.Invoke(_type);
        }

        private void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(HandleBuy);
            if (_sellButton != null)
                _sellButton.onClick.RemoveListener(HandleSell);
        }
    }
}
