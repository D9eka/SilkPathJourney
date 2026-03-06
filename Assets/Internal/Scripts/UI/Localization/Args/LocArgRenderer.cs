using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Internal.Scripts.UI.Localization.Args
{
    public static class LocArgRenderer
    {
        private static readonly Regex ArgPattern = new(@"\{(\w+)\}");

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
    }
}
