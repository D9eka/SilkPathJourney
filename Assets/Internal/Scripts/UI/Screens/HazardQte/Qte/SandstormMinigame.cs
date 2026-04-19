using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class SandstormMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _dangerZone;
        [SerializeField] private float _defaultWindSpeed = 80f;
        [SerializeField] private float _clickPush = 30f;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _alive;

        private InputRouter _inputRouter;
        private float _windSpeed;
        private float _pushAmount;
        private float _dangerLeft;
        private bool _alive;

        public void Show(IHazardInputConfig config, InputRouter inputRouter)
        {
            _inputRouter = inputRouter;
            _alive = true;

            if (config is WindResistInputConfig wr)
            {
                _windSpeed = wr.WindSpeed;
                _pushAmount = wr.ClickPush;
            }
            else
            {
                _windSpeed = _defaultWindSpeed;
                _pushAmount = _clickPush;
            }

            _cart.anchoredPosition = Vector2.zero;

            _dangerLeft = _dangerZone.rectTransform.anchoredPosition.x
                        + _dangerZone.rectTransform.rect.width * 0.5f;

            _inputRouter.EnableQteInput();
            _inputRouter.OnQteClick += OnClick;
        }

        public void Hide()
        {
            if (_inputRouter == null) return;
            _inputRouter.OnQteClick -= OnClick;
            _inputRouter.DisableQteInput();
            _inputRouter = null;
        }

        private void Update()
        {
            if (!_alive || _cart == null) return;

            var pos = _cart.anchoredPosition;
            pos.x -= _windSpeed * Time.unscaledDeltaTime;
            _cart.anchoredPosition = pos;

            if (pos.x <= _dangerLeft)
                Complete();
        }

        private void OnClick()
        {
            if (!_alive) return;
            _cart.anchoredPosition += new Vector2(_pushAmount, 0f);
        }

        private void Complete()
        {
            if (!_alive) return;
            _alive = false;
            Hide();
            OnCompleted?.Invoke(false);
        }
    }
}
