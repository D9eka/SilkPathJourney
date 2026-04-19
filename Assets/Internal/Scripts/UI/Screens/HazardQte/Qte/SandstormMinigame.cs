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

        private IQteInput _input;
        private float _windSpeed;
        private float _pushAmount;
        private float _dangerLeft;
        private bool _alive;

        public void Show(IHazardInputConfig config, IQteInput input)
        {
            _input = input;
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
