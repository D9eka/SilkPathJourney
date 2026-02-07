using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    public class ResourceIndicator : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _changeText;
        [SerializeField] private LayoutVisibilityAnimator _changeAnimator;

        [Header("Colors")]
        [SerializeField] private Color _positiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _negativeColor = new Color(0.9f, 0.2f, 0.2f, 1f);

        private Color _normalValueColor;
        private bool _cachedColor;

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

        public void SetChange(int change)
        {
            if (_changeText == null) return;

            if (change == 0)
            {
                HideChange();
                return;
            }

            string sign = change > 0 ? "+" : "";
            _changeText.text = $"{sign}{change}";
            _changeText.color = change > 0 ? _positiveColor : _negativeColor;
            _changeAnimator?.Show();
        }

        public void SetChange(float change, string format = "0.##")
        {
            if (_changeText == null) return;

            if (Mathf.Approximately(change, 0f))
            {
                HideChange();
                return;
            }

            string sign = change > 0f ? "+" : "";
            _changeText.text = $"{sign}{change.ToString(format)}";
            _changeText.color = change > 0f ? _negativeColor : _positiveColor;
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

        public void SetResource(Sprite icon, int value)
        {
            SetIcon(icon);
            SetValue(value);
            HideChange();
        }

        public void SetResourceWithPreview(Sprite icon, int currentValue, int change)
        {
            SetIcon(icon);
            SetValue(currentValue);
            SetChange(change);
        }
    }
}
