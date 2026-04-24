using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.Input
{
    public sealed class UiInputRouter : IUiInput, IInitializable, IDisposable
    {
        private readonly PlayerInputActions _actions;

        private Vector2 _navigateValue;

        public event Action<Vector2> OnNavigate;
        public event Action OnSubmit;
        public event Action OnSubmitAll;
        public event Action OnBack;
        public event Action OnAction;
        public event Action OnNextArea;
        public event Action OnPrevArea;

        public Vector2 NavigateValue => _navigateValue;

        public UiInputRouter(PlayerInputActions actions)
        {
            _actions = actions;
        }

        public void Initialize()
        {
            _actions.UI.Enable();
            _actions.UI.Navigate.performed += HandleNavigate;
            _actions.UI.Navigate.canceled  += HandleNavigate;
            _actions.UI.Submit.performed     += HandleSubmit;
            _actions.UI.SubmitAll.performed  += HandleSubmitAll;
            _actions.UI.Back.performed       += HandleBack;
            _actions.UI.Action.performed     += HandleAction;
            _actions.UI.NextArea.performed   += HandleNextArea;
            _actions.UI.PrevArea.performed   += HandlePrevArea;
        }

        public void Dispose()
        {
            _actions.UI.Navigate.performed -= HandleNavigate;
            _actions.UI.Navigate.canceled  -= HandleNavigate;
            _actions.UI.Submit.performed     -= HandleSubmit;
            _actions.UI.SubmitAll.performed  -= HandleSubmitAll;
            _actions.UI.Back.performed       -= HandleBack;
            _actions.UI.Action.performed     -= HandleAction;
            _actions.UI.NextArea.performed   -= HandleNextArea;
            _actions.UI.PrevArea.performed   -= HandlePrevArea;
            _actions.UI.Disable();
        }

        private void HandleNavigate(InputAction.CallbackContext ctx)
        {
            _navigateValue = ctx.ReadValue<Vector2>();
            OnNavigate?.Invoke(_navigateValue);
        }

        private void HandleSubmit(InputAction.CallbackContext ctx)    => OnSubmit?.Invoke();
        private void HandleSubmitAll(InputAction.CallbackContext ctx) => OnSubmitAll?.Invoke();
        private void HandleBack(InputAction.CallbackContext ctx)      => OnBack?.Invoke();
        private void HandleAction(InputAction.CallbackContext ctx)    => OnAction?.Invoke();
        private void HandleNextArea(InputAction.CallbackContext ctx)  => OnNextArea?.Invoke();
        private void HandlePrevArea(InputAction.CallbackContext ctx)  => OnPrevArea?.Invoke();
    }
}
