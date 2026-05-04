using System;
using DG.Tweening;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Event
{
    public class EventChoiceButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _conditionText;

        private Action _onClickCallback;
        private Action _onHoverEnter;
        private Action _onHoverExit;
        private HoverReporter _hover;
        private LocalizationService.LocalizedTextHandle _textHandle;

        private void Awake()
        {
            _hover = HoverReporter.GetOrAdd(gameObject);
            _hover.Entered += OnHoverEnter;
            _hover.Exited += OnHoverExit;
        }

        public void Initialize(
            LocalizationService localization,
            LocalizedString localizedText,
            Action onClickCallback,
            Action onHoverEnter = null,
            Action onHoverExit = null,
            ConditionContent condition = null)
        {
            _textHandle?.Dispose();
            _textHandle = null;
            _button.onClick.RemoveAllListeners();

            DOTween.Kill(transform);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            _onClickCallback = onClickCallback;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;

            if (localizedText != null && localization != null)
                _textHandle = localization.BindText(_text, localizedText, "Choice");

            bool hasCondition = condition != null;
            _conditionText.gameObject.SetActive(hasCondition);
            if (hasCondition)
                _conditionText.text = LocArgRenderer.Format(condition.Format, condition.Args);

            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_hover != null)
            {
                _hover.Entered -= OnHoverEnter;
                _hover.Exited -= OnHoverExit;
            }
            _textHandle?.Dispose();
            _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            _onClickCallback?.Invoke();
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        private void OnHoverEnter() => _onHoverEnter?.Invoke();
        private void OnHoverExit() => _onHoverExit?.Invoke();
    }
}
