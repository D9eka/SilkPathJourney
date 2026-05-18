using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Tooltip
{
    public class TooltipView : MonoBehaviour
    {
        private const float FADE_IN_DURATION = 0.2f;
        private const float FADE_OUT_DURATION = 0.15f;

        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _background;
        [SerializeField] private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            }
        }

        public void Show(string title, string description)
        {
            _titleText.text = title;
            _descriptionText.text = description;

            gameObject.SetActive(true);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            DOTween.Kill(_canvasGroup);
            _canvasGroup.DOFade(1f, FADE_IN_DURATION).SetEase(Ease.OutQuad);
        }

        public void Hide()
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.DOFade(0f, FADE_OUT_DURATION)
                .SetEase(Ease.InQuad)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void SetPosition(Vector2 screenPosition)
        {
            _rectTransform.position = ClampToScreen(screenPosition);
        }

        private Vector2 ClampToScreen(Vector2 position)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Vector2 tooltipSize = _rectTransform.sizeDelta;
            Vector2 pivot = _rectTransform.pivot;

            float minX = tooltipSize.x * pivot.x;
            float maxX = screenWidth - tooltipSize.x * (1f - pivot.x);
            float minY = tooltipSize.y * pivot.y;
            float maxY = screenHeight - tooltipSize.y * (1f - pivot.y);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }
    }
}
