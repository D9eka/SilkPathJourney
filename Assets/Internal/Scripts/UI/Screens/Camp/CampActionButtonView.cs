using System;
using Internal.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Camp
{
    public class CampActionButtonView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private IconLabelView _effectLabel;
        [SerializeField] private IconLabelView _costLabel;
        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _canvasGroup;

        private Action _onClick;

        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void Initialize(CampActionButtonState state, Action onClick)
        {
            _onClick = onClick;
            _nameText.text = state.Name;

            bool hasEffect = !string.IsNullOrEmpty(state.Effect.Label);
                _effectLabel.gameObject.SetActive(hasEffect);
            if (hasEffect)
            {
                _effectLabel.Initialize(state.Effect.Icon, state.Effect.Label);
            }
                
            _costLabel.Initialize(state.Cost.Icon, state.Cost.Label);

            bool hasHint = !string.IsNullOrEmpty(state.HintText);
            _hintText.gameObject.SetActive(hasHint);
            if (hasHint)
            {
                _hintText.text = state.HintText;
            }

            _canvasGroup.alpha = state.IsAvailable ? 1f : 0.5f;

            _button.interactable = state.IsAvailable;
        }

        private void HandleClick() => _onClick?.Invoke();
    }
}
