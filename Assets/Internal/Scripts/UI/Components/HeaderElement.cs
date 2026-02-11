using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public class HeaderElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

        private Color _normalColor;
        private bool _cachedColor;

        private void Awake() => CacheColor();

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void SetText(string value)
        {
            if (_text != null)
                _text.text = value ?? string.Empty;
        }

        public void SetHighlight(bool highlighted)
        {
            CacheColor();
            if (_text != null)
                _text.color = highlighted ? _highlightColor : _normalColor;
        }

        public TextMeshProUGUI Text => _text;

        private void CacheColor()
        {
            if (_cachedColor) return;
            if (_text != null) _normalColor = _text.color;
            _cachedColor = true;
        }
    }
}
