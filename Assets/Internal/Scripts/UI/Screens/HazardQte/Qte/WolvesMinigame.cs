using System;
using DG.Tweening;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class WolvesMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private Image[] _clickDots;
        [SerializeField] private Color _activeColor = Color.red;
        [SerializeField] private Color _defeatedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private float _punchScale = 0.3f;
        [SerializeField] private float _punchDuration = 0.2f;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _clickCount >= _required;

        private InputRouter _inputRouter;
        private int _clickCount;
        private int _required;
        private bool _active;

        public void Show(IHazardInputConfig config, InputRouter inputRouter)
        {
            _inputRouter = inputRouter;
            _active = true;
            _clickCount = 0;

            _required = config is MultiClickInputConfig mc ? mc.RequiredClicks : _clickDots.Length;

            foreach (var dot in _clickDots)
                if (dot != null) dot.color = _activeColor;

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

        private void OnClick()
        {
            if (!_active) return;

            if (_clickCount < _clickDots.Length)
            {
                var dot = _clickDots[_clickCount];
                if (dot != null)
                {
                    dot.color = _defeatedColor;
                    dot.transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration).SetUpdate(true);
                }
            }

            _clickCount++;

            if (_clickCount >= _required)
            {
                _active = false;
                Hide();
                OnCompleted?.Invoke(true);
            }
        }
    }
}
