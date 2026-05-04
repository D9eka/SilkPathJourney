using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    [RequireComponent(typeof(CanvasGroup), typeof(LayoutElement))]
    public class LayoutVisibilityAnimator : MonoBehaviour
    {
        [SerializeField] private float _showDuration = 0.2f;
        [SerializeField] private float _hideDuration = 0.15f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private Ease _hideEase = Ease.InCubic;

        private CanvasGroup _canvasGroup;
        private LayoutElement _layoutElement;
        private Sequence _sequence;
        private float _targetWidth;

        private bool _initialized;

        private void Awake()
        {
            EnsureInitialized();
            _layoutElement.preferredWidth = 0f;
            _canvasGroup.alpha = 0f;
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _canvasGroup = GetComponent<CanvasGroup>();
            _layoutElement = GetComponent<LayoutElement>();
            _targetWidth = _layoutElement.preferredWidth;
            _initialized = true;
        }

        public void Show()
        {
            EnsureInitialized();
            KillSequence();
            _layoutElement.preferredWidth = _targetWidth;
            _sequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase))
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        public void Hide()
        {
            EnsureInitialized();
            KillSequence();
            _sequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase))
                .SetLink(gameObject)
                .SetUpdate(true)
                .OnComplete(() => _layoutElement.preferredWidth = 0f);
        }

        public void ShowImmediate()
        {
            EnsureInitialized();
            KillSequence();
            _layoutElement.preferredWidth = _targetWidth;
            _canvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            EnsureInitialized();
            KillSequence();
            _layoutElement.preferredWidth = 0f;
            _canvasGroup.alpha = 0f;
        }

        public void SetTargetWidth(float width)
        {
            _targetWidth = width;
        }

        private void KillSequence()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();
            _sequence = null;
        }

        private void OnDestroy()
        {
            KillSequence();
        }
    }
}
