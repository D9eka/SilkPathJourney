using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    [AddComponentMenu("Layout/Flow Layout Group", 154)]
    public class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField] private float _spacingX;
        [SerializeField] private float _spacingY;
        [SerializeField] private bool _childForceExpandWidth;

        private readonly List<Row> _rows = new();

        private struct Row
        {
            public int StartIndex;
            public int EndIndex;
            public float Width;
            public float Height;
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            float maxChildWidth = 0f;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                float childMin = LayoutUtility.GetPreferredWidth(rectChildren[i]);
                if (childMin > maxChildWidth) maxChildWidth = childMin;
            }
            float minWidth = maxChildWidth + padding.left + padding.right;
            SetLayoutInputForAxis(minWidth, minWidth, -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            BuildRows();
            float totalHeight = padding.top + padding.bottom;
            for (int i = 0; i < _rows.Count; i++)
            {
                totalHeight += _rows[i].Height;
                if (i < _rows.Count - 1) totalHeight += _spacingY;
            }
            SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            BuildRows();
            ApplyLayout();
        }

        public override void SetLayoutVertical()
        {
            ApplyLayout();
        }

        private void BuildRows()
        {
            _rows.Clear();
            if (rectChildren.Count == 0) return;

            float availableWidth = rectTransform.rect.width - padding.left - padding.right;

            int rowStart = 0;
            float rowWidth = 0f;
            float rowHeight = 0f;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childWidth = LayoutUtility.GetPreferredWidth(child);
                float childHeight = LayoutUtility.GetPreferredHeight(child);

                bool isFirstInRow = i == rowStart;
                float nextWidth = isFirstInRow ? childWidth : rowWidth + _spacingX + childWidth;

                if (!isFirstInRow && nextWidth > availableWidth)
                {
                    _rows.Add(new Row
                    {
                        StartIndex = rowStart,
                        EndIndex = i - 1,
                        Width = rowWidth,
                        Height = rowHeight
                    });
                    rowStart = i;
                    rowWidth = childWidth;
                    rowHeight = childHeight;
                }
                else
                {
                    rowWidth = nextWidth;
                    if (childHeight > rowHeight) rowHeight = childHeight;
                }
            }

            _rows.Add(new Row
            {
                StartIndex = rowStart,
                EndIndex = rectChildren.Count - 1,
                Width = rowWidth,
                Height = rowHeight
            });
        }

        private void ApplyLayout()
        {
            if (_rows.Count == 0) return;

            float availableWidth = rectTransform.rect.width - padding.left - padding.right;
            float availableHeight = rectTransform.rect.height - padding.top - padding.bottom;

            float totalHeight = 0f;
            for (int i = 0; i < _rows.Count; i++)
            {
                totalHeight += _rows[i].Height;
                if (i < _rows.Count - 1) totalHeight += _spacingY;
            }

            float alignmentY = GetVerticalAlignmentFactor();
            float startY = padding.top + (availableHeight - totalHeight) * alignmentY;

            float alignmentX = GetHorizontalAlignmentFactor();

            float y = startY;
            for (int r = 0; r < _rows.Count; r++)
            {
                Row row = _rows[r];
                float extra = availableWidth - row.Width;
                float perChildExtra = 0f;
                float rowStartX;

                int count = row.EndIndex - row.StartIndex + 1;

                if (_childForceExpandWidth && count > 0 && extra > 0f)
                {
                    perChildExtra = extra / count;
                    rowStartX = padding.left;
                }
                else
                {
                    rowStartX = padding.left + extra * alignmentX;
                }

                float x = rowStartX;
                for (int i = row.StartIndex; i <= row.EndIndex; i++)
                {
                    RectTransform child = rectChildren[i];
                    float childWidth = LayoutUtility.GetPreferredWidth(child) + perChildExtra;
                    float childHeight = LayoutUtility.GetPreferredHeight(child);

                    float offsetY = (row.Height - childHeight) * alignmentY;

                    SetChildAlongAxis(child, 0, x, childWidth);
                    SetChildAlongAxis(child, 1, y + offsetY, childHeight);

                    x += childWidth + _spacingX;
                }

                y += row.Height + _spacingY;
            }
        }

        private float GetHorizontalAlignmentFactor()
        {
            switch (childAlignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.LowerLeft:
                    return 0f;
                case TextAnchor.UpperCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.LowerCenter:
                    return 0.5f;
                default:
                    return 1f;
            }
        }

        private float GetVerticalAlignmentFactor()
        {
            switch (childAlignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    return 0f;
                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    return 0.5f;
                default:
                    return 1f;
            }
        }
    }
}
