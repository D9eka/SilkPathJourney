using System;
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

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => false;

        private IQteInput _input;
        private float _pulseSpeed;
        private float _threshold;
        private float _elapsed;
        private bool _active;

        public void Show(IMinigameConfig config, IQteInput input)
        {
            _input = input;
            _active = true;
            _elapsed = 0f;

            var tc = config as RiverMinigameConfig;
            if (tc == null) { Debug.LogError($"[RiverMinigame] bad config: {config?.GetType().Name}"); OnCompleted?.Invoke(false); return; }
            _pulseSpeed = tc.PulseSpeed;
            _threshold = tc.CalmThreshold;

            _input.Enable();
            _input.OnClick += OnClick;
        }

        public void Hide()
        {
            if (_input == null) return;
            _input.OnClick -= OnClick;
            _input.Disable();
            _input = null;
        }

        private void Update()
        {
            if (!_active || _river == null) return;

            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.PingPong(_elapsed * _pulseSpeed, 1f);

            Vector3 scale = _river.localScale;
            scale.y = Mathf.Lerp(_pulseMinScaleY, _pulseMaxScaleY, t);
            _river.localScale = scale;

            _riverImage.color = Color.Lerp(_calmColor, _roughColor, t);
        }

        private void OnClick()
        {
            if (!_active) return;

            float t = Mathf.PingPong(_elapsed * _pulseSpeed, 1f);
            bool calm = t < _threshold;

            _active = false;
            Hide();
            OnCompleted?.Invoke(calm);
        }
    }
}
