using System;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    public class CloseButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        public event Action Clicked;

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null)
                _button.interactable = interactable;
        }

        public Button Button => _button;

        private void HandleClick() => Clicked?.Invoke();
    }
}
