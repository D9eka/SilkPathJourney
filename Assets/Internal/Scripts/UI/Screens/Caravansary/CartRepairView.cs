using System;
using Internal.Scripts.UI.Screens.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Caravansary
{
    public class CartRepairView : MonoBehaviour, IOfferingItemView
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _repairButton;
        [SerializeField] private TextMeshProUGUI _repairText;
        [SerializeField] private Button _repairMaxButton;
        [SerializeField] private TextMeshProUGUI _repairMaxText;
        [SerializeField] private GameObject _buttonsContainer;

        private Action _onRepair;
        private Action _onRepairMax;
        private bool _listenersAdded;

        public void Initialize(OfferingItem item, Action onAction, Action onActionMax)
        {
            Initialize(item.RepairData, onAction, onActionMax);
        }

        public void Initialize(CartRepairEntry data, Action onRepair, Action onRepairMax)
        {
            _onRepair = onRepair;
            _onRepairMax = onRepairMax;

            gameObject.SetActive(data.IsVisible);
            if (!data.IsVisible) return;

            if (_titleText != null)
                _titleText.text = data.Title;

            if (_durabilityText != null)
                _durabilityText.text = data.DurabilityText;

            if (_priceText != null)
                _priceText.text = data.PriceText;

            bool bothDisabled = !data.CanRepair && !data.CanRepairMax;

            if (_buttonsContainer != null)
                _buttonsContainer.SetActive(!bothDisabled);

            if (bothDisabled)
            {
                if (_repairButton != null)
                {
                    _repairButton.gameObject.SetActive(true);
                    _repairButton.interactable = false;
                    if (!_listenersAdded)
                        _repairButton.onClick.AddListener(HandleRepair);
                }

                if (_repairText != null)
                    _repairText.text = data.RepairButtonText;

                if (_repairMaxButton != null)
                    _repairMaxButton.gameObject.SetActive(false);
            }
            else
            {
                if (_repairButton != null)
                {
                    _repairButton.gameObject.SetActive(true);
                    _repairButton.interactable = data.CanRepair;
                    if (!_listenersAdded)
                        _repairButton.onClick.AddListener(HandleRepair);
                }

                if (_repairText != null)
                    _repairText.text = data.RepairButtonText;

                if (_repairMaxButton != null)
                {
                    _repairMaxButton.gameObject.SetActive(true);
                    _repairMaxButton.interactable = data.CanRepairMax;
                    if (!_listenersAdded)
                        _repairMaxButton.onClick.AddListener(HandleRepairMax);
                }

                if (_repairMaxText != null)
                    _repairMaxText.text = data.RepairMaxButtonText;
            }

            _listenersAdded = true;
        }

        private void HandleRepair()
        {
            _onRepair?.Invoke();
        }

        private void HandleRepairMax()
        {
            _onRepairMax?.Invoke();
        }

        private void OnDestroy()
        {
            if (_repairButton != null)
                _repairButton.onClick.RemoveListener(HandleRepair);
            if (_repairMaxButton != null)
                _repairMaxButton.onClick.RemoveListener(HandleRepairMax);
        }
    }
}
