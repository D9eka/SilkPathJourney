namespace Internal.Scripts.WorldModifiers
{
    public enum ModifierStaleness
    {
        Actual,
        Stale,
        Unknown
    }

    public static class ModifierStalenessHelper
    {
        private const float StaleAlpha = 0.5f;

        public static ModifierStaleness GetStaleness(int lastSeenDay, int currentDay,
            int actualDays = 20, int staleDays = 40)
        {
            if (lastSeenDay < 0) return ModifierStaleness.Unknown;
            int diff = currentDay - lastSeenDay;
            if (diff <= actualDays) return ModifierStaleness.Actual;
            if (diff <= staleDays) return ModifierStaleness.Stale;
            return ModifierStaleness.Unknown;
        }

        public static float GetAlpha(ModifierStaleness staleness)
        {
            return staleness == ModifierStaleness.Stale ? StaleAlpha : 1f;
        }

        public const string UnknownTitle = "???";

        public static string GetUnknownDescription()
        {
            bool isRu = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale?.Identifier.Code?.StartsWith("ru") == true;
            return isRu ? "Неизвестный эффект" : "Unknown effect";
        }
    }
}
