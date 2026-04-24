using System;
using DG.Tweening;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class RiverMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _river;
        [SerializeField] private Image _riverImage;
        [SerializeField] private Color _calmColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color _roughColor = new Color(0.2f, 0.3f, 0.8f);
        [SerializeField] private float _pulseMinScaleY = 0.5f;
        [SerializeField] private float _pulseMaxScaleY = 1.5f;

        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _cartImage;
        [SerializeField] private RectTransform _upAnchor;
        [SerializeField] private RectTransform _downAnchor;

        public event Action<bool> OnCompleted;

        private enum State { Idle, Stepping, Sinking, Done }

        private IQteInput _input;
        private RiverMinigameConfig _cfg;
        private float _threshold;
        private float _phase;
        private float _currentPulseSpeed;
        private bool _active;

        private State _state;
        private bool _won;
        private bool _startFromUp;
        private Sequence _tween;

        private bool _cached;
        private Vector3 _initialScale;
        private Color _initialColor;
        private Vector2 _startAnchored;
        private Vector2 _endAnchored;

        public bool DidPlayerSucceed() => _won;

        public void Show(IMinigameConfig config, IQteInput input)
        {
            _input = input;
            _active = true;
            _phase = 0f;

            var tc = config as RiverMinigameConfig;
            if (tc == null) { Debug.LogError($"[RiverMinigame] bad config: {config?.GetType().Name}"); OnCompleted?.Invoke(false); return; }
            _cfg = tc;
            _threshold = tc.CalmThreshold;
            _currentPulseSpeed = UnityEngine.Random.Range(tc.MinPulseSpeed, tc.MaxPulseSpeed);

            if (!_cached)
            {
                _cached = true;
                _initialScale = _cart.localScale;
                _initialColor = _cartImage.color;
            }
            else
            {
                _cart.localScale = _initialScale;
                _cartImage.color = _initialColor;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_cart.parent);

            // Выравниваем anchors Cart с Up/Down, чтобы anchoredPosition
            // читался в одной системе координат.
            _cart.anchorMin = _upAnchor.anchorMin;
            _cart.anchorMax = _upAnchor.anchorMax;
            _cart.pivot = _upAnchor.pivot;

            _startFromUp = UnityEngine.Random.value < 0.5f;
            Vector2 upAnchored = _upAnchor.anchoredPosition;
            Vector2 downAnchored = _downAnchor.anchoredPosition;

            _startAnchored = _startFromUp ? upAnchored : downAnchored;
            _endAnchored = _startFromUp ? downAnchored : upAnchored;

            _cart.anchoredPosition = _startAnchored;
            _state = State.Idle;
            _won = false;

            _input.Enable();
            _input.OnClick += OnClick;
        }

        public void Hide()
        {
            _active = false;
            _tween?.Kill();
            if (_cached)
            {
                _cart.localScale = _initialScale;
                _cartImage.color = _initialColor;
            }
            if (_input == null) return;
            _input.OnClick -= OnClick;
            _input.Disable();
            _input = null;
        }

        private void Update()
        {
            if (!_active || _river == null) return;
            if (_state != State.Idle) return;

            float prev = _phase;
            _phase += _currentPulseSpeed * Time.unscaledDeltaTime;

            if (Mathf.FloorToInt(prev * 2f) != Mathf.FloorToInt(_phase * 2f))
                _currentPulseSpeed = UnityEngine.Random.Range(_cfg.MinPulseSpeed, _cfg.MaxPulseSpeed);

            float t = Mathf.PingPong(_phase, 1f);

            Vector3 scale = _river.localScale;
            scale.y = Mathf.Lerp(_pulseMinScaleY, _pulseMaxScaleY, t);
            _river.localScale = scale;

            _riverImage.color = Color.Lerp(_calmColor, _roughColor, t);
        }

        private void OnClick()
        {
            if (_state != State.Idle) return;

            float t = Mathf.PingPong(_phase, 1f);
            bool calm = t < _threshold;

            if (calm)
            {
                _state = State.Stepping;

                _tween?.Kill();
                _tween = DOTween.Sequence();
                _tween.Append(_cart.DOAnchorPos(_endAnchored, _cfg.StepDuration));
                _tween.AppendInterval(_cfg.CompletionDelay);
                _tween.OnComplete(() =>
                {
                    _won = true;
                    _state = State.Done;
                    _active = false;
                    Hide();
                    OnCompleted?.Invoke(true);
                });
            }
            else
            {
                _state = State.Sinking;
                Vector2 mid = (_startAnchored + _endAnchored) * 0.5f;
                Color drownColor = new Color(0.15f, 0.2f, 0.45f, _initialColor.a);

                _tween?.Kill();
                _tween = DOTween.Sequence();
                _tween.Append(_cart.DOAnchorPos(mid, _cfg.SinkMoveDuration));
                _tween.Append(_cart.DOScale(0.3f, _cfg.SinkScaleDuration));
                _tween.Join(_cartImage.DOColor(drownColor, _cfg.SinkScaleDuration));
                _tween.AppendInterval(_cfg.CompletionDelay);
                _tween.OnComplete(() =>
                {
                    _state = State.Done;
                    _active = false;
                    Hide();
                    OnCompleted?.Invoke(false);
                });
            }
        }
    }
}
