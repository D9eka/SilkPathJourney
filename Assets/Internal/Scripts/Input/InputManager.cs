using Internal.Scripts.InteractableObjects;
using Internal.Scripts.InteractableObjects.Abstraction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.Input
{
    public class InputManager : IInitializable, ITickable, System.IDisposable
    {
        private readonly UnityEngine.Camera _mainCamera;
        private PlayerInputActions _inputActions;
        private IClickable _currentHover;

        [Inject]
        public InputManager(UnityEngine.Camera mainCamera)
        {
            _mainCamera = mainCamera;
        }

        public void Initialize()
        {
            _inputActions = new PlayerInputActions();
            EnableInput();
        }

        public void Dispose()
        {
            DisableInput();
        }

        public void Tick()
        {
            HandleHover();
        }

        private void EnableInput()
        {
            _inputActions.Enable();
            _inputActions.Player.Click.performed += OnClick;
        }

        private void DisableInput()
        {
            _inputActions.Player.Click.performed -= OnClick;
            _inputActions.Disable();
        }

        private void HandleHover()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            
            RaycastHit2D hit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(screenPos));

            if (hit.collider != null && hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                if (_currentHover != clickable)
                {
                    _currentHover?.OnHoverExit();
                    _currentHover = clickable;
                    _currentHover.OnHoverEnter();
                }
            }
            else
            {
                _currentHover?.OnHoverExit();
                _currentHover = null;
            }
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            
            RaycastHit2D hit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(screenPos));

            if (hit.collider != null && hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                clickable.OnClick();
            }
        }
    }
}
