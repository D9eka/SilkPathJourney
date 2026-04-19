namespace Internal.Scripts.UI.Utils
{
    public static class NumberFormatter
    {
        public static string Signed(int v) => v >= 0 ? $"+{v}" : v.ToString();
        public static string SignedPercent(float v) => v >= 0 ? $"+{v:F0}%" : $"{v:F0}%";
    }
}
