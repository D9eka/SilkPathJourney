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
        private float _scrollbarHeight;
        private bool _scrollbarHeightCached;
        private Coroutine _refreshCoroutine;

        protected override void OnEnable()
        {
            verticalScrollbarVisibility = ScrollbarVisibility.Permanent;
            horizontalScrollbarVisibility = ScrollbarVisibility.Permanent;
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

        private float GetScrollbarHeight()
        {
            if (!_scrollbarHeightCached && horizontalScrollbar != null)
            {
                float h = ((RectTransform)horizontalScrollbar.transform).rect.height;
                if (h > 0)
                {
                    _scrollbarHeight = h;
                    _scrollbarHeightCached = true;
                }
            }
            return _scrollbarHeight;
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
            if (content == null || viewport == null || 
                (verticalScrollbar == null && horizontalScrollbar == null)) return;

            if (verticalScrollbar != null)
            {
                UpdateVerticalScrollbarVisibility();
            }

            if (horizontalScrollbar != null)
            {
                UpdateHorizontalScrollbarVisibility();
            }
        }

        private void UpdateVerticalScrollbarVisibility()
        {
            bool needsVerticalScroll = content.rect.height > viewport.rect.height;
            float expectedWidthOffset = needsVerticalScroll ? -(GetScrollbarWidth() + _scrollbarPadding) : 0;

            if (verticalScrollbar.gameObject.activeSelf != needsVerticalScroll)
                verticalScrollbar.gameObject.SetActive(needsVerticalScroll);

            if (!Mathf.Approximately(viewport.offsetMax.x, expectedWidthOffset))
                viewport.offsetMax = new Vector2(expectedWidthOffset, viewport.offsetMax.y);
        }

        private void UpdateHorizontalScrollbarVisibility()
        {
            bool needsHorizontalScroll = content.rect.width > viewport.rect.width;
            float expectedBottomOffset = needsHorizontalScroll ? GetScrollbarHeight() + _scrollbarPadding : 0;

            if (horizontalScrollbar.gameObject.activeSelf != needsHorizontalScroll)
                horizontalScrollbar.gameObject.SetActive(needsHorizontalScroll);

            if (!Mathf.Approximately(viewport.offsetMin.y, expectedBottomOffset))
                viewport.offsetMin = new Vector2(viewport.offsetMin.x, expectedBottomOffset);
        }
    }
}
