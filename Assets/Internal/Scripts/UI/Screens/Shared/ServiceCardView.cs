using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Shared
{
    public class ServiceCardView : MonoBehaviour, IOfferingItemView
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;

        private Action _callback;

        public void Initialize(OfferingItem item, Action onAction, Action onActionMax)
        {
            Initialize(item.Title, item.Description, item.ButtonText, item.CanAction, onAction);
        }

        public void Initialize(string title, string description, string buttonText, bool interactable, Action callback)
        {
            _callback = callback;

            if (_titleText != null)
                _titleText.text = title;

            if (_descriptionText != null)
                _descriptionText.text = description;

            if (_buttonText != null)
                _buttonText.text = buttonText;

            if (_button != null)
            {
                _button.interactable = interactable;
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            _callback?.Invoke();
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }
    }
}
