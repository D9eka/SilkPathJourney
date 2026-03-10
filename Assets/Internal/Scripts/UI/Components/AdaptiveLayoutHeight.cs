using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    [RequireComponent(typeof(LayoutElement))]
    public class AdaptiveLayoutHeight : MonoBehaviour
    {
        [Tooltip("Элемент, чью реальную высоту отслеживаем")]
        [SerializeField] private RectTransform _trackedContent;

        private LayoutElement _layoutElement;
        private RectTransform _rectTransform;
        private Coroutine _refreshCoroutine;

        private void Awake()
        {
            _layoutElement = GetComponent<LayoutElement>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Refresh()
        {
            if (_refreshCoroutine != null)
                StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = StartCoroutine(RefreshNextFrame());
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;

            float contentHeight = LayoutUtility.GetPreferredHeight(_trackedContent);
            float available = GetAvailableHeight();

            if (contentHeight <= available)
            {
                _layoutElement.preferredHeight = contentHeight;
                _layoutElement.flexibleHeight = -1;
            }
            else
            {
                _layoutElement.preferredHeight = -1;
                _layoutElement.flexibleHeight = 1;
            }

            _refreshCoroutine = null;
        }

        private float GetAvailableHeight()
        {
            var parent = _rectTransform.parent as RectTransform;
            if (parent == null) return float.MaxValue;

            float total = parent.rect.height;
            var layoutGroup = parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
            float spacing = layoutGroup != null ? layoutGroup.spacing : 0;
            float padding = layoutGroup != null
                ? layoutGroup.padding.top + layoutGroup.padding.bottom : 0;

            float siblingsHeight = 0;
            int siblingCount = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i) as RectTransform;
                if (child == _rectTransform || !child.gameObject.activeSelf) continue;
                siblingsHeight += LayoutUtility.GetPreferredHeight(child);
                siblingCount++;
            }

            float totalSpacing = siblingCount > 0 ? spacing * siblingCount : 0;
            return total - siblingsHeight - totalSpacing - padding;
        }
    }
}
