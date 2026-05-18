using System;
using Internal.Scripts.InteractableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.Input
{
    public class InputRouter : ICameraInput, ITimeSpeedInput, IInitializable, ITickable, IDisposable
    {
        private const float CAMERA_SIZE_MODIFIER = -1f;
        private const float MAX_RAY_DISTANCE = 1000f;
        private const int RAYCAST_BUFFER_SIZE = 8;

        private static readonly Vector2 CameraMovementModifier = new Vector2(-1f, -1f);

        public event Action<float>   OnChangeSize;
        public event Action<Vector2> OnChangePosition;
        public event Action OnPause;
        public event Action OnSpeed1;
        public event Action OnSpeed2;
        public event Action OnSpeed3;

        private readonly PlayerInputActions _actions;

        private LayerMask _interactableLayerMask;
        private IInteractableObject _currentHover;
        private readonly RaycastHit[] _raycastBuffer = new RaycastHit[RAYCAST_BUFFER_SIZE];

        private bool _clickRequested;
        private float _zoomValue;
        private Vector2 _moveValue;

        public InputRouter(PlayerInputActions actions)
        {
            _actions = actions;
        }

        public void SetInteractableLayerMask(LayerMask mask)
        {
            _interactableLayerMask = mask;
        }

        public void Initialize()
        {
            _actions.Player.Enable();

            _actions.Player.Click.performed          += OnClick;
            _actions.Player.ZoomCamera.performed     += OnZoomCamera;
            _actions.Player.ZoomCamera.canceled      += OnZoomCamera;
            _actions.Player.MoveCamera.performed     += OnMoveCamera;
            _actions.Player.MoveCamera.canceled      += OnMoveCamera;
            _actions.Player.TimeSpeedPause.performed += OnTimeSpeedPauseAction;
            _actions.Player.TimeSpeed1.performed     += OnTimeSpeed1Action;
            _actions.Player.TimeSpeed2.performed     += OnTimeSpeed2Action;
            _actions.Player.TimeSpeed3.performed     += OnTimeSpeed3Action;
        }

        public void Dispose()
        {
            _actions.Player.Click.performed          -= OnClick;
            _actions.Player.ZoomCamera.performed     -= OnZoomCamera;
            _actions.Player.ZoomCamera.canceled      -= OnZoomCamera;
            _actions.Player.MoveCamera.performed     -= OnMoveCamera;
            _actions.Player.MoveCamera.canceled      -= OnMoveCamera;
            _actions.Player.TimeSpeedPause.performed -= OnTimeSpeedPauseAction;
            _actions.Player.TimeSpeed1.performed     -= OnTimeSpeed1Action;
            _actions.Player.TimeSpeed2.performed     -= OnTimeSpeed2Action;
            _actions.Player.TimeSpeed3.performed     -= OnTimeSpeed3Action;

            _actions.Player.Disable();
        }

        public void Tick()
        {
            bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            HandleHover(isOverUI);
            HandleClick(isOverUI);
            HandleCamera(isOverUI);

            _clickRequested = false;
            _zoomValue = 0;
        }

        private void HandleHover(bool isOverUI)
        {
            if (isOverUI)
            {
                if (_currentHover != null)
                {
                    _currentHover.TriggerHoverExit();
                    _currentHover = null;
                }
                return;
            }

            IInteractableObject view = TryGetInteractableUnderMouse();

            if (_currentHover != view)
            {
                _currentHover?.TriggerHoverExit();
                _currentHover = view;
                _currentHover?.TriggerHoverEnter();
            }
        }

        private void HandleClick(bool isOverUI)
        {
            if (!_clickRequested || isOverUI)
                return;

            IInteractableObject view = TryGetInteractableUnderMouse();
            view?.TriggerClick();
        }

        private void HandleCamera(bool isOverUI)
        {
            if (isOverUI)
                return;

            OnChangeSize?.Invoke(_zoomValue * CAMERA_SIZE_MODIFIER);
            OnChangePosition?.Invoke(_moveValue * CameraMovementModifier);
        }

        private IInteractableObject TryGetInteractableUnderMouse()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null || Mouse.current == null)
                return null;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(screenPos);

            int count = Physics.RaycastNonAlloc(ray, _raycastBuffer, MAX_RAY_DISTANCE, _interactableLayerMask);
            if (count == 0)
                return null;

            if (count == RAYCAST_BUFFER_SIZE)
                Debug.LogWarning("[InputRouter] RaycastNonAlloc buffer full — increase RAYCAST_BUFFER_SIZE");


            IInteractableObject best = null;
            int bestPriority = int.MinValue;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var hit = _raycastBuffer[i];
                if (!hit.collider.TryGetComponent(out IInteractableObject view))
                    continue;

                int priority = view.InteractionPriority;
                if (priority > bestPriority || (priority == bestPriority && hit.distance < bestDistance))
                {
                    best = view;
                    bestPriority = priority;
                    bestDistance = hit.distance;
                }
            }

            return best;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed)
                _clickRequested = true;
        }

        private void OnZoomCamera(InputAction.CallbackContext context)
        {
            _zoomValue = context.canceled ? 0f : context.ReadValue<float>();
        }

        private void OnMoveCamera(InputAction.CallbackContext context)
        {
            _moveValue = context.canceled ? Vector2.zero : context.ReadValue<Vector2>();
        }

        private void OnTimeSpeedPauseAction(InputAction.CallbackContext context) => OnPause?.Invoke();
        private void OnTimeSpeed1Action(InputAction.CallbackContext context)     => OnSpeed1?.Invoke();
        private void OnTimeSpeed2Action(InputAction.CallbackContext context)     => OnSpeed2?.Invoke();
        private void OnTimeSpeed3Action(InputAction.CallbackContext context)     => OnSpeed3?.Invoke();
    }
}
