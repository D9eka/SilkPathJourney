using System;
using UnityEngine.InputSystem;

namespace Internal.Scripts.Input
{
    public sealed class QteInputRouter : IQteInput, IDisposable
    {
        private readonly PlayerInputActions _actions;
        private bool _enabled;

        public event Action OnClick;
        public event Action OnClickCanceled;
        public event Action OnLeft;
        public event Action OnRight;
        public event Action OnDown;
        public event Action OnUp;

        public InputAction ClickAction => _actions.QTE.Click;
        public InputAction LeftAction  => _actions.QTE.Left;
        public InputAction RightAction => _actions.QTE.Right;
        public InputAction DownAction  => _actions.QTE.Down;
        public InputAction UpAction    => _actions.QTE.Up;

        public QteInputRouter(PlayerInputActions actions)
        {
            _actions = actions;
        }

        public void Enable()
        {
            if (_enabled) return;
            _enabled = true;

            _actions.Player.Disable();
            _actions.QTE.Enable();

            _actions.QTE.Click.performed += HandleClick;
            _actions.QTE.Click.canceled  += HandleClickCanceled;
            _actions.QTE.Left.performed  += HandleLeft;
            _actions.QTE.Right.performed += HandleRight;
            _actions.QTE.Down.performed  += HandleDown;
            _actions.QTE.Up.performed    += HandleUp;
        }

        public void Disable()
        {
            if (!_enabled) return;
            _enabled = false;

            _actions.QTE.Click.performed -= HandleClick;
            _actions.QTE.Click.canceled  -= HandleClickCanceled;
            _actions.QTE.Left.performed  -= HandleLeft;
            _actions.QTE.Right.performed -= HandleRight;
            _actions.QTE.Down.performed  -= HandleDown;
            _actions.QTE.Up.performed    -= HandleUp;

            _actions.QTE.Disable();
            _actions.Player.Enable();
        }

        public void Dispose()
        {
            if (_enabled) Disable();
        }

        private void HandleClick(InputAction.CallbackContext ctx)        => OnClick?.Invoke();
        private void HandleClickCanceled(InputAction.CallbackContext ctx)=> OnClickCanceled?.Invoke();
        private void HandleLeft(InputAction.CallbackContext ctx)         => OnLeft?.Invoke();
        private void HandleRight(InputAction.CallbackContext ctx)        => OnRight?.Invoke();
        private void HandleDown(InputAction.CallbackContext ctx)         => OnDown?.Invoke();
        private void HandleUp(InputAction.CallbackContext ctx)           => OnUp?.Invoke();
    }
}
