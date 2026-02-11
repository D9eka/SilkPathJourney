using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Colors")]
        [SerializeField] private Color _positiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _negativeColor = new Color(0.9f, 0.2f, 0.2f, 1f);

        public ResourceType ResourceType => _resourceType;

        public void SetResourceType(ResourceType type) => _resourceType = type;

        private Color _normalValueColor;
        private bool _cachedColor;
        private Sequence _autoHideSequence;

        protected virtual void Awake()
        {
            if (_valueText != null && !_cachedColor)
            {
                _normalValueColor = _valueText.color;
                _cachedColor = true;
            }
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
        }

        private void OnDestroy()
        {
            KillAutoHide();
        }
    }
}
