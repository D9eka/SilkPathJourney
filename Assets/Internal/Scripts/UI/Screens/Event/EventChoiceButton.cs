using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Event
{
    public class EventChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _conditionText;

        private Action _onClickCallback;
        private Action _onHoverEnter;
        private Action _onHoverExit;
        private LocalizationService.LocalizedTextHandle _textHandle;

        public void Initialize(
            LocalizationService localization,
            LocalizedString localizedText,
            Action onClickCallback,
            Action onHoverEnter = null,
            Action onHoverExit = null,
            ConditionContent condition = null)
        {
            _onClickCallback = onClickCallback;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;

            if (_text != null && localizedText != null && localization != null)
                _textHandle = localization.BindText(_text, localizedText, "Choice");

            if (_conditionText != null)
            {
                bool hasCondition = condition != null;
                _conditionText.gameObject.SetActive(hasCondition);
                if (hasCondition)
                    _conditionText.text = LocArgRenderer.Format(condition.Format, condition.Args);
            }

            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _textHandle?.Dispose();
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            _onClickCallback?.Invoke();
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null)
                _button.interactable = interactable;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverExit?.Invoke();
        }
    }
}
