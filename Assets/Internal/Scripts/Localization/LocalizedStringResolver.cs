using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Localization
{
    internal static class LocalizedStringResolver
    {
        public static string Resolve(LocalizedString localized, string fallback, string context)
        {
            if (localized == null)
                return fallback;

            try
            {
                string value = localized.GetLocalizedString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to resolve LocalizedString for {context}: {e.Message}");
            }

            return fallback;
        }
    }
}
