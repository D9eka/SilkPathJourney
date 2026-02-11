using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _fillRect;
        [SerializeField] private RectTransform _fillArea;

        private float _areaWidth;
        private float _pendingRatio;
        private bool _initialized;
        private Tween _fillTween;

        private async void Start()
        {
            await UniTask.WaitForEndOfFrame(this);
            _areaWidth = _fillArea.rect.width;
            _initialized = true;
            ApplyFill(_pendingRatio);
        }

        public void SetFill(float ratio)
        {
            _pendingRatio = Mathf.Clamp01(ratio);
            if (_initialized)
                ApplyFill(_pendingRatio);
        }

        public void AnimateFill(float ratio, float duration = 0.5f)
        {
            _pendingRatio = Mathf.Clamp01(ratio);
            if (!_initialized) return;

            _fillTween?.Kill();
            float targetWidth = _areaWidth * _pendingRatio;
            _fillTween = DOTween.To(
                () => _fillRect.sizeDelta.x,
                x => _fillRect.sizeDelta = new Vector2(x, _fillRect.sizeDelta.y),
                targetWidth, duration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void ApplyFill(float ratio)
        {
            _fillRect.sizeDelta = new Vector2(_areaWidth * ratio, _fillRect.sizeDelta.y);
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
        }
    }
}
