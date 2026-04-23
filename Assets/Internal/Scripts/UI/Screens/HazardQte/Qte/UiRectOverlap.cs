using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public static class UiRectOverlap
    {
        private static readonly Vector3[] CornerBuffer = new Vector3[4];

        public static bool Check(RectTransform a, RectTransform b)
            => GetWorldRect(a).Overlaps(GetWorldRect(b));

        private static Rect GetWorldRect(RectTransform rt)
        {
            rt.GetWorldCorners(CornerBuffer);
            float xMin = Mathf.Min(CornerBuffer[0].x, CornerBuffer[2].x);
            float yMin = Mathf.Min(CornerBuffer[0].y, CornerBuffer[2].y);
            float xMax = Mathf.Max(CornerBuffer[0].x, CornerBuffer[2].x);
            float yMax = Mathf.Max(CornerBuffer[0].y, CornerBuffer[2].y);
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }
}
