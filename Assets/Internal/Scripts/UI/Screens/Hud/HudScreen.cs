using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screen.View;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Hud
{
    public class HudScreen : ScreenViewBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _openInventoryText;
        [SerializeField] private TextMeshProUGUI _enterCityText;
        [SerializeField] private TextMeshProUGUI _startMoveText;
        [Header("Buttons")]
        [SerializeField] private Button _openInventoryButton;
        [SerializeField] private Button _enterCityButton;
        [SerializeField] private Button _startMoveButton;
        [SerializeField] private Button _cancelMoveButton;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _openInventoryLocalizedString;
        [SerializeField] private LocalizedString _enterCityLocalizedString;
        [SerializeField] private LocalizedString _startMoveLocalizedString;

        private UnityAction _openInventoryHandler;
        private UnityAction _enterCityHandler;
        private UnityAction _startMoveHandler;
        private UnityAction _cancelMoveHandler;
        private LocalizationHelper.LocalizedTextHandle _openInventoryHandle;
        private LocalizationHelper.LocalizedTextHandle _enterCityHandle;
        private LocalizationHelper.LocalizedTextHandle _startMoveHandle;
        private LocalizationHelper.LocalizedTextHandle _cancelMoveHandle;

        private void OnEnable()
        {
            BindLocalization();
        }

        private void OnDisable()
        {
            _openInventoryHandle?.Dispose();
            _openInventoryHandle = null;
            _enterCityHandle?.Dispose();
            _enterCityHandle = null;
            _startMoveHandle?.Dispose();
            _startMoveHandle = null;
            _cancelMoveHandle?.Dispose();
            _cancelMoveHandle = null;
        }

        public void BindOpenInventory(Action action)
        {
            UnbindOpenInventory();
            if (action == null)
                return;

            _openInventoryHandler = () => action.Invoke();
            _openInventoryButton.onClick.AddListener(_openInventoryHandler);
        }

        public void BindEnterCity(Action action)
        {
            UnbindEnterCity();
            if (action == null)
                return;

            _enterCityHandler = () => action.Invoke();
            _enterCityButton.onClick.AddListener(_enterCityHandler);
        }

        public void BindStartMove(Action action)
        {
            UnbindStartMove();
            if (action == null)
                return;

            _startMoveHandler = () => action.Invoke();
            _startMoveButton.onClick.AddListener(_startMoveHandler);
        }

        public void BindCancelMove(Action action)
        {
            UnbindCancelMove();
            if (action == null)
                return;

            _cancelMoveHandler = () => action.Invoke();
            _cancelMoveButton.onClick.AddListener(_cancelMoveHandler);
        }

        public void UnbindAll()
        {
            UnbindOpenInventory();
            UnbindEnterCity();
            UnbindStartMove();
            UnbindCancelMove();
        }

        public void SetEnterCityVisible(bool state)
        {
            _enterCityButton.gameObject.SetActive(state);
        }

        public void SetStartMoveVisible(bool state)
        {
            _startMoveButton.gameObject.SetActive(state);
        }

        public void SetCancelMoveVisible(bool state)
        {
            _cancelMoveButton.gameObject.SetActive(state);
        }

        public void SetInteractable(bool state)
        {
            _openInventoryButton.interactable = state;
            _enterCityButton.interactable = state;
            _startMoveButton.interactable = state;
            _cancelMoveButton.interactable = state;
        }

        private void UnbindOpenInventory()
        {
            if (_openInventoryHandler == null)
                return;

            _openInventoryButton.onClick.RemoveListener(_openInventoryHandler);
            _openInventoryHandler = null;
        }

        private void UnbindEnterCity()
        {
            if (_enterCityHandler == null)
                return;

            _enterCityButton.onClick.RemoveListener(_enterCityHandler);
            _enterCityHandler = null;
        }

        private void UnbindStartMove()
        {
            if (_startMoveHandler == null)
                return;

            _startMoveButton.onClick.RemoveListener(_startMoveHandler);
            _startMoveHandler = null;
        }

        private void UnbindCancelMove()
        {
            if (_cancelMoveHandler == null)
                return;

            _cancelMoveButton.onClick.RemoveListener(_cancelMoveHandler);
            _cancelMoveHandler = null;
        }

        private void BindLocalization()
        {
            _openInventoryHandle?.Dispose();
            _enterCityHandle?.Dispose();
            _startMoveHandle?.Dispose();
            _cancelMoveHandle?.Dispose();
            _openInventoryHandle = LocalizationHelper.BindText(_openInventoryText, _openInventoryLocalizedString, $"{name}.OpenInventory");
            _enterCityHandle = LocalizationHelper.BindText(_enterCityText, _enterCityLocalizedString, $"{name}.EnterCity");
            _startMoveHandle = LocalizationHelper.BindText(_startMoveText, _startMoveLocalizedString, $"{name}.StartMove");
        }
    }
}
