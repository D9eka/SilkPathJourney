using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;

namespace Internal.Scripts.UI.Localization.Args
{
    public static class LocArgRenderer
    {
        private static readonly Regex ArgPattern = new(@"\{(\w+)\}");
        private static readonly Regex SpeechPattern = new(
            @"<npc_speech\s+lang=""(\w+)"">(.+?)</npc_speech>", RegexOptions.Singleline);

        public static string Format(string format, IReadOnlyList<ILocArg> args)
        {
            if (string.IsNullOrEmpty(format) || args == null || args.Count == 0)
                return format ?? string.Empty;

            var argMap = new Dictionary<string, ILocArg>(args.Count);
            foreach (var arg in args)
                argMap[arg.Key] = arg;

            return ArgPattern.Replace(format, match =>
            {
                string key = match.Groups[1].Value;
                return argMap.TryGetValue(key, out var arg) ? arg.ToMarkup() : match.Value;
            });
        }

        public static string ProcessNpcSpeech(string text, PlayerLanguageRepository repo)
        {
            if (string.IsNullOrEmpty(text) || repo == null || !text.Contains("<npc_speech"))
                return text;

            return SpeechPattern.Replace(text, match =>
            {
                if (!Enum.TryParse(match.Groups[1].Value, out LanguageType langType))
                    return match.Groups[2].Value;

                var proficiency = repo.Current.GetProficiency(langType);
                return NpcSpeechLocArg.ToMarkup(match.Groups[2].Value, proficiency);
            });
        }
    }
}
