using DG.Tweening;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class ObstacleMinigame : MinigameBase
    {
        [SerializeField] private RectTransform _road;
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _obstacle1;
        [SerializeField] private Image _obstacle2;
        [SerializeField] private RectTransform _topLane;
        [SerializeField] private RectTransform _bottomLane;
        [SerializeField] private GameObject _hintUp;
        [SerializeField] private GameObject _hintDown;

        private const float LaneSwitchDuration = 0.2f;

        private IQteInput _input;
        private float _cartSpeed;
        private float _screenRight;
        private float _topY;
        private float _bottomY;
        private float _cartStartX;
        private bool _obstacle1OnTop;
        private bool _obstacle2OnTop;

        private void Awake()
        {
            _cartStartX = _cart.anchoredPosition.x;
        }

        public override void Show(IMinigameConfig config, IQteInput input)
        {
            _input = input;
            SetAlive(true);

            var d = config as DodgeMinigameConfig;
            if (d == null) { Debug.LogError($"[ObstacleMinigame] bad config: {config?.GetType().Name}"); Complete(false); return; }
            _cartSpeed = d.CartSpeed;
            float reactionMargin = d.ReactionMargin;
            float escapeMargin = d.EscapeMargin;
            float segmentMargin = d.SegmentMargin;

            Canvas.ForceUpdateCanvases();

            _topY = _topLane.anchoredPosition.y;
            _bottomY = _bottomLane.anchoredPosition.y;
            _screenRight = _road.rect.width;

            _obstacle1OnTop = UnityEngine.Random.value < 0.5f;
            _obstacle2OnTop = !_obstacle1OnTop;

            float min1 = _cartStartX + reactionMargin;
            float max1 = _screenRight - escapeMargin - segmentMargin;
            float x1;
            float x2;
            if (max1 <= min1 || _screenRight - escapeMargin <= min1 + segmentMargin)
            {
                Debug.LogError($"[ObstacleMinigame] invalid X layout, width={_screenRight}, cartStart={_cartStartX}, reaction={reactionMargin}, escape={escapeMargin}, segment={segmentMargin}");
                float mid = _screenRight * 0.5f;
                x1 = mid - segmentMargin * 0.5f;
                x2 = mid + segmentMargin * 0.5f;
            }
            else
            {
                x1 = UnityEngine.Random.Range(min1, max1);
                float min2 = x1 + segmentMargin;
                float max2 = _screenRight - escapeMargin;
                x2 = UnityEngine.Random.Range(min2, max2);
            }

            _obstacle1.rectTransform.anchoredPosition = new Vector2(x1, _obstacle1OnTop ? _topY : _bottomY);
            _obstacle2.rectTransform.anchoredPosition = new Vector2(x2, _obstacle2OnTop ? _topY : _bottomY);
            _cart.anchoredPosition = new Vector2(_cartStartX, _obstacle1OnTop ? _topY : _bottomY);

            UpdateHint();

            _input.Enable();
            _input.OnUp += OnUpPressed;
            _input.OnDown += OnDownPressed;
        }

        public override void Hide()
        {
            if (_input == null) return;
            _input.OnUp -= OnUpPressed;
            _input.OnDown -= OnDownPressed;
            _input.Disable();
            _input = null;
            _hintUp.SetActive(false);
            _hintDown.SetActive(false);
        }

        private void Update()
        {
            if (!IsAlive) return;

            var pos = _cart.anchoredPosition;
            pos.x += _cartSpeed * Time.unscaledDeltaTime;
            _cart.anchoredPosition = pos;

            if (UiRectOverlap.Check(_cart, _obstacle1.rectTransform) || UiRectOverlap.Check(_cart, _obstacle2.rectTransform))
            {
                Complete(false);
                return;
            }

            if (pos.x + _cart.rect.width * 0.5f >= _screenRight)
            {
                Complete(true);
                return;
            }

            UpdateHint();
        }

        private void OnUpPressed()
        {
            if (!IsAlive) return;
            if (Mathf.Approximately(_cart.anchoredPosition.y, _topY)) return;
            _cart.DOAnchorPosY(_topY, LaneSwitchDuration).SetUpdate(true);
        }

        private void OnDownPressed()
        {
            if (!IsAlive) return;
            if (Mathf.Approximately(_cart.anchoredPosition.y, _bottomY)) return;
            _cart.DOAnchorPosY(_bottomY, LaneSwitchDuration).SetUpdate(true);
        }

        private void UpdateHint()
        {
            bool? nextLane = GetNextObstacleLane();
            if (!nextLane.HasValue)
            {
                _hintUp.SetActive(false);
                _hintDown.SetActive(false);
                return;
            }

            bool needTop = !nextLane.Value;
            bool cartOnTop = Mathf.Approximately(_cart.anchoredPosition.y, _topY);
            if (needTop == cartOnTop)
            {
                _hintUp.SetActive(false);
                _hintDown.SetActive(false);
                return;
            }

            _hintUp.SetActive(needTop);
            _hintDown.SetActive(!needTop);
        }

        private bool? GetNextObstacleLane()
        {
            float cartX = _cart.anchoredPosition.x;
            if (cartX < _obstacle1.rectTransform.anchoredPosition.x) return _obstacle1OnTop;
            if (cartX < _obstacle2.rectTransform.anchoredPosition.x) return _obstacle2OnTop;
            return null;
        }
    }
}
