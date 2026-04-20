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
        [SerializeField] private RectTransform _road;
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _obstacle1;
        [SerializeField] private Image _obstacle2;
        [SerializeField] private RectTransform _topLane;
        [SerializeField] private RectTransform _bottomLane;
        [SerializeField] private float _defaultCartSpeed = 100f;
        [SerializeField] private GameObject _hintUp;
        [SerializeField] private GameObject _hintDown;

        private const float LaneSwitchDuration = 0.2f;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _succeeded;

        private static readonly Vector3[] CornerBuffer = new Vector3[4];

        private IQteInput _input;
        private float _cartSpeed;
        private bool _alive;
        private bool _succeeded;
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

        public void Show(IHazardInputConfig config, IQteInput input)
        {
            _input = input;
            _alive = true;
            _succeeded = false;

            var d = config as DodgeInputConfig;
            _cartSpeed = d != null ? d.CartSpeed : _defaultCartSpeed;
            float reactionMargin = d != null ? d.ReactionMargin : 120f;
            float escapeMargin = d != null ? d.EscapeMargin : 80f;
            float segmentMargin = d != null ? d.SegmentMargin : 120f;

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

        public void Hide()
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
            if (!_alive) return;

            var pos = _cart.anchoredPosition;
            pos.x += _cartSpeed * Time.unscaledDeltaTime;
            _cart.anchoredPosition = pos;

            if (CollidesWith(_obstacle1) || CollidesWith(_obstacle2))
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
            if (!_alive) return;
            if (Mathf.Approximately(_cart.anchoredPosition.y, _topY)) return;
            _cart.DOAnchorPosY(_topY, LaneSwitchDuration).SetUpdate(true);
        }

        private void OnDownPressed()
        {
            if (!_alive) return;
            if (Mathf.Approximately(_cart.anchoredPosition.y, _bottomY)) return;
            _cart.DOAnchorPosY(_bottomY, LaneSwitchDuration).SetUpdate(true);
        }

        private void Complete(bool success)
        {
            if (!_alive) return;
            _alive = false;
            _succeeded = success;
            Hide();
            OnCompleted?.Invoke(success);
        }

        private bool CollidesWith(Image obstacle)
        {
            return GetWorldRect(_cart).Overlaps(GetWorldRect(obstacle.rectTransform));
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

        private static Rect GetWorldRect(RectTransform rt)
        {
            rt.GetWorldCorners(CornerBuffer);
            Vector2 min = CornerBuffer[0];
            Vector2 max = CornerBuffer[2];
            return new Rect(min, max - min);
        }
    }
}
