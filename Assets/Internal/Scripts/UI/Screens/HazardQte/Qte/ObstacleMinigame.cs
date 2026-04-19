using System;
using DG.Tweening;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class ObstacleMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _obstacle;
        [SerializeField] private RectTransform _mainRoad;
        [SerializeField] private RectTransform _bypassRoad;
        [SerializeField] private float _defaultCartSpeed = 100f;

        private const float ScreenRightMargin = 100f;
        private const float LaneSwitchDuration = 0.2f;
        private const int PhaseApproach = 0;
        private const int PhaseBypass = 1;
        private const int PhaseReturn = 2;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _phase == PhaseReturn;

        private InputRouter _inputRouter;
        private float _cartSpeed;
        private int _phase;
        private bool _alive;
        private float _obstacleX;
        private float _screenRight;
        private Vector2 _cartStartPosition;

        private void Awake()
        {
            _cartStartPosition = _cart.anchoredPosition;
        }

        public void Show(IHazardInputConfig config, InputRouter inputRouter)
        {
            _inputRouter = inputRouter;
            _alive = true;
            _phase = PhaseApproach;

            _cartSpeed = config is ClickInputConfig c ? c.CartSpeed : _defaultCartSpeed;

            _cart.anchoredPosition = _cartStartPosition;

            _obstacleX = _obstacle.rectTransform.anchoredPosition.x;
            _screenRight = _mainRoad.rect.width * 0.5f + ScreenRightMargin;

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
            pos.x += _cartSpeed * Time.unscaledDeltaTime;
            _cart.anchoredPosition = pos;

            if (_phase == PhaseApproach && pos.x >= _obstacleX)
            {
                Complete(false);
                return;
            }

            if (_phase == PhaseBypass && pos.x >= _screenRight)
                Complete(false);
        }

        private void OnClick()
        {
            if (!_alive) return;

            if (_phase == PhaseApproach)
            {
                _phase = PhaseBypass;
                float bypassY = _bypassRoad.anchoredPosition.y;
                _cart.DOLocalMoveY(bypassY, LaneSwitchDuration).SetUpdate(true);
            }
            else if (_phase == PhaseBypass)
            {
                _phase = PhaseReturn;
                _cart.DOLocalMoveY(0f, LaneSwitchDuration).SetUpdate(true).OnComplete(() => Complete(true));
            }
        }

        private void Complete(bool success)
        {
            if (!_alive) return;
            _alive = false;
            Hide();
            OnCompleted?.Invoke(success);
        }
    }
}
