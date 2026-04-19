using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class RockfallMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private RectTransform _safeZoneLeft;
        [SerializeField] private RectTransform _safeZoneRight;
        [SerializeField] private float _defaultMoveSpeed = 150f;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => IsInSafeZone();

        private IQteInput _input;
        private float _moveSpeed;
        private bool _active;
        private Vector2 _cartStartPosition;
        private bool _hasStartPosition;

        private static readonly Vector3[] CornerBuffer = new Vector3[4];

        private void Awake()
        {
            if (_cart != null)
            {
                _cartStartPosition = _cart.anchoredPosition;
                _hasStartPosition = true;
            }
        }

        public void Show(IHazardInputConfig config, IQteInput input)
        {
            _input = input;
            _active = true;

            _moveSpeed = config is LeftOrRightInputConfig lr ? lr.MoveSpeed : _defaultMoveSpeed;

            if (_cart != null && _hasStartPosition)
                _cart.anchoredPosition = _cartStartPosition;

            _input.Enable();
        }

        public void Hide()
        {
            if (_input == null) return;
            _input.Disable();
            _input = null;
        }

        private void Update()
        {
            if (!_active || _cart == null || _input == null) return;

            float dir = 0f;
            if (_input.LeftAction != null && _input.LeftAction.IsPressed())
                dir -= 1f;
            if (_input.RightAction != null && _input.RightAction.IsPressed())
                dir += 1f;

            if (dir != 0f)
            {
                Vector2 pos = _cart.anchoredPosition;
                pos.x += dir * _moveSpeed * Time.unscaledDeltaTime;
                _cart.anchoredPosition = pos;
            }

            if (IsInSafeZone())
            {
                _active = false;
                Hide();
                OnCompleted?.Invoke(true);
            }
        }

        private bool IsInSafeZone()
        {
            Rect cartRect = GetWorldRect(_cart);
            if (_safeZoneLeft != null && cartRect.Overlaps(GetWorldRect(_safeZoneLeft))) return true;
            if (_safeZoneRight != null && cartRect.Overlaps(GetWorldRect(_safeZoneRight))) return true;
            return false;
        }

        private static Rect GetWorldRect(RectTransform rt)
        {
            rt.GetWorldCorners(CornerBuffer);
            Vector2 min = CornerBuffer[0];
            Vector2 max = CornerBuffer[2];
            return new Rect(min, max - min);
        }
    }
}
