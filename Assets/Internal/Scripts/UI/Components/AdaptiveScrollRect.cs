using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    public class AdaptiveScrollRect : ScrollRect
    {
        [SerializeField] private float _scrollbarPadding = 12f;

        private float _scrollbarWidth;
        private bool _scrollbarWidthCached;
        private Coroutine _refreshCoroutine;

        protected override void OnEnable()
        {
            verticalScrollbarVisibility = ScrollbarVisibility.Permanent;
            base.OnEnable();
        }

        private float GetScrollbarWidth()
        {
            if (!_scrollbarWidthCached && verticalScrollbar != null)
            {
                float w = ((RectTransform)verticalScrollbar.transform).rect.width;
                if (w > 0)
                {
                    _scrollbarWidth = w;
                    _scrollbarWidthCached = true;
                }
            }
            return _scrollbarWidth;
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
            UpdateScrollbarVisibility();
            _refreshCoroutine = null;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            UpdateScrollbarVisibility();
        }

        private void UpdateScrollbarVisibility()
        {
            if (content == null || viewport == null || verticalScrollbar == null) return;

            bool needsScroll = content.rect.height > viewport.rect.height;
            float expectedOffset = needsScroll ? -(GetScrollbarWidth() + _scrollbarPadding) : 0;

            if (verticalScrollbar.gameObject.activeSelf != needsScroll)
                verticalScrollbar.gameObject.SetActive(needsScroll);

            if (!Mathf.Approximately(viewport.offsetMax.x, expectedOffset))
                viewport.offsetMax = new Vector2(expectedOffset, viewport.offsetMax.y);
        }
    }
}
