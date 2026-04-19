using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class SlopeMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _cliff;
        [SerializeField] private float _defaultSlideSpeed = 60f;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _alive;

        private InputRouter _inputRouter;
        private float _slideSpeed;
        private float _cliffBottom;
        private bool _holding;
        private bool _alive;

        public void Show(IHazardInputConfig config, InputRouter inputRouter)
        {
            _inputRouter = inputRouter;
            _alive = true;
            _holding = false;

            _slideSpeed = config is HoldClickInputConfig hc ? hc.SlideSpeed : _defaultSlideSpeed;

            _cart.anchoredPosition = Vector2.zero;

            _cliffBottom = _cliff.rectTransform.anchoredPosition.y
                         + _cliff.rectTransform.rect.height * 0.5f;

            _inputRouter.EnableQteInput();
            _inputRouter.OnQteClick += OnHoldStart;
            _inputRouter.OnQteClickCanceled += OnHoldStop;
        }

        public void Hide()
        {
            if (_inputRouter == null) return;
            _inputRouter.OnQteClick -= OnHoldStart;
            _inputRouter.OnQteClickCanceled -= OnHoldStop;
            _inputRouter.DisableQteInput();
            _inputRouter = null;
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
        private void OnHoldStop() => _holding = false;

        private void Complete()
        {
            if (!_alive) return;
            _alive = false;
            Hide();
            OnCompleted?.Invoke(false);
        }
    }
}
