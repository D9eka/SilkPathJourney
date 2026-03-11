using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.UI
{
    public sealed class UIScreenRoots : MonoBehaviour
    {
        private const float OVERLAY_FADE_DURATION = 0.3f;
        private const float OVERLAY_TARGET_ALPHA = 0.5f;

        [Header("Roots")]
        [SerializeField] private Transform _screensRoot;

        [Header("Overlay")]
        [SerializeField] private CanvasGroup _dimOverlay;

        private Canvas _dimOverlayCanvas;
        private Tween _dimOverlayTween;

        public Transform ScreensRoot => _screensRoot;

        private void Awake()
        {
            if (_dimOverlay != null)
                _dimOverlayCanvas = _dimOverlay.GetComponent<Canvas>();
        }

        public void SetDimOverlayVisible(bool visible, int sortOrder = 0)
        {
            if (_dimOverlay == null) return;

            _dimOverlayTween?.Kill();

            float target = visible ? OVERLAY_TARGET_ALPHA : 0f;
            if (_dimOverlayCanvas != null)
                _dimOverlayCanvas.sortingOrder = sortOrder;
            _dimOverlay.blocksRaycasts = visible;
            _dimOverlayTween = _dimOverlay
                .DOFade(target, OVERLAY_FADE_DURATION)
                .SetEase(visible ? Ease.OutCubic : Ease.InCubic)
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void OnDestroy()
        {
            _dimOverlayTween?.Kill();
        }
    }
}
