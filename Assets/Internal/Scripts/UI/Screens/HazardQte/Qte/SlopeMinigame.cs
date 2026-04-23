using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class SlopeMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _cliff;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _alive;

        private IQteInput _input;
        private float _slideSpeed;
        private float _cliffBottom;
        private bool _holding;
        private bool _alive;

        public void Show(IMinigameConfig config, IQteInput input)
        {
            _input = input;
            _alive = true;
            _holding = false;

            var hc = config as SlopeMinigameConfig;
            if (hc == null) { Debug.LogError($"[SlopeMinigame] bad config: {config?.GetType().Name}"); OnCompleted?.Invoke(false); return; }
            _slideSpeed = hc.SlideSpeed;

            _cart.anchoredPosition = Vector2.zero;

            _cliffBottom = _cliff.rectTransform.anchoredPosition.y
                         + _cliff.rectTransform.rect.height * 0.5f;

            _input.Enable();
            _input.OnClick        += OnHoldStart;
            _input.OnClickCanceled += OnHoldStop;
        }

        public void Hide()
        {
            if (_input == null) return;
            _input.OnClick        -= OnHoldStart;
            _input.OnClickCanceled -= OnHoldStop;
            _input.Disable();
            _input = null;
        }

        private void Update()
        {
            if (!_alive || _holding || _cart == null) return;

            var pos = _cart.anchoredPosition;
            pos.y -= _slideSpeed * Time.unscaledDeltaTime;
            _cart.anchoredPosition = pos;

            if (pos.y <= _cliffBottom)
                Complete();
        }

        private void OnHoldStart() => _holding = true;
        private void OnHoldStop()  => _holding = false;

        private void Complete()
        {
            if (!_alive) return;
            _alive = false;
            Hide();
            OnCompleted?.Invoke(false);
        }
    }
}
