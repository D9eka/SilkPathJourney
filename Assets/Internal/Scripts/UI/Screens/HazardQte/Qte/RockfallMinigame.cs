using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class RockfallMinigame : MinigameBase
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private RectTransform _safeZoneLeft;
        [SerializeField] private RectTransform _safeZoneRight;

        private IQteInput _input;
        private float _moveSpeed;
        private Vector2 _cartStartPosition;
        private bool _hasStartPosition;

        private void Awake()
        {
            if (_cart != null)
            {
                _cartStartPosition = _cart.anchoredPosition;
                _hasStartPosition = true;
            }
        }

        public override void Show(IMinigameConfig config, IQteInput input)
        {
            _input = input;
            SetAlive(true);

            var lr = config as RockfallMinigameConfig;
            if (lr == null) { Debug.LogError($"[RockfallMinigame] bad config: {config?.GetType().Name}"); Complete(false); return; }
            _moveSpeed = lr.MoveSpeed;

            if (_cart != null && _hasStartPosition)
                _cart.anchoredPosition = _cartStartPosition;

            _input.Enable();
        }

        public override void Hide()
        {
            if (_input == null) return;
            _input.Disable();
            _input = null;
        }

        private void Update()
        {
            if (!IsAlive || _cart == null || _input == null) return;

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
                Complete(true);
        }

        private bool IsInSafeZone()
        {
            if (_safeZoneLeft != null && UiRectOverlap.Check(_cart, _safeZoneLeft)) return true;
            if (_safeZoneRight != null && UiRectOverlap.Check(_cart, _safeZoneRight)) return true;
            return false;
        }
    }
}
