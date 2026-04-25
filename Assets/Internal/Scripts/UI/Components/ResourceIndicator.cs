using DG.Tweening;
using Internal.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Components
{
    public class ResourceIndicator : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private ResourceType _resourceType;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _changeText;
        [SerializeField] private LayoutVisibilityAnimator _changeAnimator;

        [Inject(Optional = true)] private UiThemeService _themeService;

        public ResourceType ResourceType => _resourceType;

        public void SetResourceType(ResourceType type) => _resourceType = type;

        private Color _positiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        private Color _negativeColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        private Color _normalValueColor;
        private bool _cachedColor;
        private Sequence _autoHideSequence;
        private Tween _valueTween;

        protected virtual void Awake()
        {
            ApplyThemeColors();

            if (_valueText != null && !_cachedColor)
            {
                _normalValueColor = _valueText.color;
                _cachedColor = true;
            }
        }

        public void Initialize(UiThemeService themeService)
        {
            _themeService = themeService;
            ApplyThemeColors();
        }

        private void ApplyThemeColors()
        {
            if (_themeService == null) return;
            _positiveColor = _themeService.GetColor(ColorSlot.ValuePositive);
            _negativeColor = _themeService.GetColor(ColorSlot.ValueNegative);
        }

        public void SetIcon(Sprite icon)
        {
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.gameObject.SetActive(icon != null);
            }
        }

        public void SetValue(int value)
        {
            if (_valueText != null)
                _valueText.text = value.ToString();
        }

        public void SetValue(string formatted)
        {
            if (_valueText != null)
                _valueText.text = formatted;
        }

        public void SetHighlight(bool highlighted)
        {
            if (_valueText == null) return;
            if (!_cachedColor)
            {
                _normalValueColor = _valueText.color;
                _cachedColor = true;
            }
            _valueText.color = highlighted ? _negativeColor : _normalValueColor;
        }

        public void SetChange(int change, bool increaseIsPositive)
        {
            if (_changeText == null) return;

            if (change == 0)
            {
                HideChange();
                return;
            }

            string sign = change > 0 ? "+" : "";
            _changeText.text = $"{sign}{change}";
            _changeText.color = GetChangeColor(change > 0, increaseIsPositive);
            _changeAnimator?.Show();
        }

        public void SetChange(float change, bool increaseIsPositive, string format = "0.##")
        {
            if (_changeText == null) return;

            if (Mathf.Approximately(change, 0f))
            {
                HideChange();
                return;
            }

            string sign = change > 0f ? "+" : "";
            _changeText.text = $"{sign}{change.ToString(format)}";
            _changeText.color = GetChangeColor(change > 0f, increaseIsPositive);
            _changeAnimator?.Show();
        }

        public void HideChange()
        {
            _changeAnimator?.Hide();
        }

        public void HideChangeImmediate()
        {
            _changeAnimator?.HideImmediate();
        }

        public void ShowTemporaryChange(int change, bool increaseIsPositive, float displayDuration = 2f)
        {
            KillAutoHide();
            SetChange(change, increaseIsPositive);
            if (change == 0) return;

            _autoHideSequence = DOTween.Sequence()
                .AppendInterval(displayDuration)
                .AppendCallback(HideChange)
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        public void AnimateValueChange(int change, bool increaseIsPositive,
            float from, float to, float duration = 3f)
        {
            KillAutoHide();
            if (change == 0)
            {
                SetValue($"{to:0}");
                HideChange();
                return;
            }

            Color changeColor = GetChangeColor(change > 0, increaseIsPositive);
            string sign = change > 0 ? "+" : "";
            if (_changeText != null) _changeText.color = changeColor;
            _changeAnimator?.Show();

            _valueTween = DOTween.To(() => 0f, t =>
            {
                float currentValue = Mathf.Lerp(from, to, t);
                SetValue($"{currentValue:0}");

                int remainingChange = Mathf.RoundToInt(Mathf.Lerp(change, 0, t));
                if (_changeText != null)
                    _changeText.text = remainingChange == 0 ? "" : $"{sign}{remainingChange}";
            }, 1f, duration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    SetValue($"{to:0}");
                    HideChange();
                });
        }

        public virtual void ApplyAnimated(float value, float maxValue, int change,
            bool increaseIsPositive, float duration)
        {
            float oldValue = value - change;
            AnimateValueChange(change, increaseIsPositive, oldValue, value, duration);
        }

        public virtual void ApplyImmediate(float value, float maxValue)
        {
            SetValue($"{value:0}");
        }

        public void SetResource(Sprite icon, int value)
        {
            SetIcon(icon);
            SetValue(value);
            HideChange();
        }

        private Color GetChangeColor(bool isIncrease, bool increaseIsPositive)
        {
            bool isGood = isIncrease == increaseIsPositive;
            return isGood ? _positiveColor : _negativeColor;
        }

        private void KillAutoHide()
        {
            if (_autoHideSequence != null && _autoHideSequence.IsActive())
                _autoHideSequence.Kill();
            _autoHideSequence = null;
            _valueTween?.Kill();
            _valueTween = null;
        }

        private void OnDestroy()
        {
            KillAutoHide();
        }
    }
}
