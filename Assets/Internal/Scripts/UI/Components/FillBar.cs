using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _fillRect;

        private Tween _fillTween;

        public void SetFill(float ratio)
        {
            ApplyFill(Mathf.Clamp01(ratio));
        }

        public void AnimateFill(float ratio, float duration = 0.5f)
        {
            ratio = Mathf.Clamp01(ratio);
            _fillTween?.Kill();
            _fillTween = DOTween.To(
                () => _fillRect.anchorMax.x,
                x => ApplyFill(x),
                ratio, duration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void ApplyFill(float ratio)
        {
            _fillRect.anchorMin = Vector2.zero;
            _fillRect.anchorMax = new Vector2(ratio, 1f);
            _fillRect.offsetMin = Vector2.zero;
            _fillRect.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
        }
    }
}
