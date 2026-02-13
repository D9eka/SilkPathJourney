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

        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descriptionText;
        private Image _background;
        private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform;

        public static TooltipView Create()
        {
            var rootGo = new GameObject("TooltipCanvas");
            DontDestroyOnLoad(rootGo);

            var canvas = rootGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            rootGo.AddComponent<CanvasScaler>();
            rootGo.AddComponent<GraphicRaycaster>();

            var canvasGroup = rootGo.AddComponent<CanvasGroup>();

            // Background panel
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(rootGo.transform, false);
            var bg = bgGo.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.16f, 0.16f, 0.88f);
            bg.raycastTarget = false;

            var csf = bgGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = bgGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Title text
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(bgGo.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 16;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            var titleLayout = titleGo.AddComponent<LayoutElement>();
            titleLayout.preferredWidth = 200;

            // Description text
            var descGo = new GameObject("DescriptionText");
            descGo.transform.SetParent(bgGo.transform, false);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 13;
            descTmp.color = new Color(0.8f, 0.8f, 0.8f);
            var descLayout = descGo.AddComponent<LayoutElement>();
            descLayout.preferredWidth = 200;

            var tooltipView = rootGo.AddComponent<TooltipView>();
            tooltipView.Initialize(titleTmp, descTmp, bg, canvasGroup);

            rootGo.SetActive(false);
            return tooltipView;
        }

        public void Initialize(TextMeshProUGUI titleText, TextMeshProUGUI descriptionText,
            Image background, CanvasGroup canvasGroup)
        {
            _titleText = titleText;
            _descriptionText = descriptionText;
            _background = background;
            _canvasGroup = canvasGroup;
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
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

            DOTween.KillAll(_canvasGroup);
            _canvasGroup.DOFade(1f, FADE_IN_DURATION).SetEase(Ease.OutQuad);
        }

        public void Hide()
        {
            DOTween.KillAll(_canvasGroup);
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
