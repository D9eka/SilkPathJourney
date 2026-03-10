using System;
using System.Collections.Generic;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class DictionaryImportExtensions
    {
        public static List<TEnum> ToSortedUniqueValues<TEnum>(
            this Dictionary<string, TEnum> map, TEnum noneValue)
            where TEnum : struct, Enum
        {
            List<string> keys = new List<string>(map.Keys);
            keys.Sort(StringComparer.Ordinal);

            List<TEnum> result = new List<TEnum>();
            foreach (string key in keys)
            {
                TEnum value = map[key];
                if (EqualityComparer<TEnum>.Default.Equals(value, noneValue))
                    continue;
                if (!result.Contains(value))
                    result.Add(value);
            }

            return result;
        }

        public static List<TValue> GetOrCreateList<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> dict, TKey key)
        {
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                dict[key] = list;
            }
            return list;
        }
    }
}
