using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.Utils
{
    public static class SurveyLink
    {
        public const string UrlRu = "https://forms.gle/8wVuXLyREomskpYk9";
        public const string UrlEn = "https://forms.gle/6U58Pb1YEBRJET1NA";

        public static string GetUrl()
        {
            string code = LocalizationSettings.SelectedLocale?.Identifier.Code;
            return code != null && code.StartsWith("ru") ? UrlRu : UrlEn;
        }

        public static void Open() => Application.OpenURL(GetUrl());
    }
}
