using Internal.Scripts.Player.Languages;

namespace Internal.Scripts.UI.Localization.Args
{
    public static class NpcSpeechLocArg
    {
        public const string UnknownLinkId = "npc_unknown";
        private const string UnknownLanguageColor = "#888888";
        private const string SpeechColor = "#C8A96E";
        private const char BlockChar = '█';
        private const float HideRatioNone = 1f;
        private const float HideRatioBasic = 0.7f;
        private const float HideRatioConversational = 0.3f;

        public static string ToMarkup(string speechText, LanguageProficiency proficiency)
        {
            string adapted = AdaptText(speechText, proficiency);
            return WrapRichText(adapted, proficiency);
        }

        private static string AdaptText(string text, LanguageProficiency proficiency)
        {
            if (string.IsNullOrEmpty(text) || proficiency >= LanguageProficiency.Fluent)
                return text ?? string.Empty;

            string[] words = text.Split(' ');
            float hideRatio = proficiency switch
            {
                LanguageProficiency.None => HideRatioNone,
                LanguageProficiency.Basic => HideRatioBasic,
                _ => HideRatioConversational
            };

            for (int i = 0; i < words.Length; i++)
            {
                if (!ShouldHide(i, words.Length, hideRatio))
                    continue;

                string word = words[i];
                int letterCount = 0;
                int trailStart = word.Length;

                for (int c = word.Length - 1; c >= 0; c--)
                {
                    if (char.IsLetterOrDigit(word[c]))
                        break;
                    trailStart = c;
                }

                foreach (char ch in word)
                {
                    if (char.IsLetterOrDigit(ch))
                        letterCount++;
                }

                if (letterCount == 0)
                    continue;

                string block = $"<link=\"{UnknownLinkId}\">{new string(BlockChar, letterCount)}</link>";
                words[i] = trailStart < word.Length
                    ? block + word[trailStart..]
                    : block;
            }

            return string.Join(' ', words);
        }

        private static bool ShouldHide(int wordIndex, int totalWords, float hideRatio)
        {
            if (totalWords <= 0) return false;
            int hiddenCount = (int)(totalWords * hideRatio);
            int step = totalWords > hiddenCount && hiddenCount > 0
                ? totalWords / hiddenCount
                : 1;
            return (wordIndex % step) < (hideRatio >= 1f ? step : 1);
        }

        private static string WrapRichText(string text, LanguageProficiency proficiency)
        {
            if (proficiency == LanguageProficiency.None)
                return $"<color={UnknownLanguageColor}>{text}</color>";

            return $"<color={SpeechColor}><i>«{text}»</i></color>";
        }
    }
}
