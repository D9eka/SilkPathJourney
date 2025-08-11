using UnityEngine;

namespace Internal.Scripts.Extensions
{
    public static class ColorExtension
    {
        public static Color GetColorWithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}