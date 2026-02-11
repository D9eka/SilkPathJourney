using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class LocalizationImporter
    {
        private const string LOCALIZATION_PREFIX = "loc_";

        public static void CollectFromCsv(string csvPath, string keyColumn,
            Dictionary<string, LocalizationEntry> entries)
        {
            if (!File.Exists(csvPath)) return;

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return;

            string[] header = rows[0];
            int keyIndex = ImportHelpers.FindColumnIndex(header, keyColumn);
            Dictionary<string, int> localeColumns = FindLocaleColumns(header);
            if (keyIndex < 0 || localeColumns.Count == 0) return;

            string csvFile = Path.GetFileName(csvPath);

            for (int i = 1; i < rows.Count; i++)
            {
                string key = ImportHelpers.GetField(rows[i], keyIndex).Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!entries.TryGetValue(key, out LocalizationEntry entry))
                {
                    entry = new LocalizationEntry(key);
                    entries[key] = entry;
                }

                foreach (var lc in localeColumns)
                {
                    string value = ImportHelpers.GetField(rows[i], lc.Value);
                    entry.SetIfNonEmpty(lc.Key, value, csvFile);
                }
            }
        }

        public static void CollectFromCsvWithPrefix(string csvPath, string keyColumn,
            string keyPrefix, Dictionary<string, LocalizationEntry> entries)
        {
            if (!File.Exists(csvPath)) return;

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return;

            string[] header = rows[0];
            int keyIndex = ImportHelpers.FindColumnIndex(header, keyColumn);
            if (keyIndex < 0) keyIndex = ImportHelpers.FindColumnIndex(header, "name_key");

            Dictionary<string, int> localeColumns = FindLocaleColumns(header);
            if (localeColumns.Count == 0)
                localeColumns = FindLocaleColumnsPlain(header);
            if (keyIndex < 0 || localeColumns.Count == 0) return;

            string csvFile = Path.GetFileName(csvPath);

            for (int i = 1; i < rows.Count; i++)
            {
                string key = ImportHelpers.GetField(rows[i], keyIndex).Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!string.IsNullOrWhiteSpace(keyPrefix) &&
                    !key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!entries.TryGetValue(key, out LocalizationEntry entry))
                {
                    entry = new LocalizationEntry(key);
                    entries[key] = entry;
                }

                foreach (var lc in localeColumns)
                {
                    string value = ImportHelpers.GetField(rows[i], lc.Value);
                    entry.SetIfNonEmpty(lc.Key, value, csvFile);
                }
            }
        }

        public static void CollectFromCsvPlainLocales(string csvPath, string keyColumn,
            string keyPrefix, Dictionary<string, LocalizationEntry> entries)
        {
            if (!File.Exists(csvPath)) return;

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return;

            string[] header = rows[0];
            int keyIndex = ImportHelpers.FindColumnIndex(header, keyColumn);
            if (keyIndex < 0) keyIndex = ImportHelpers.FindColumnIndex(header, "name_key");

            Dictionary<string, int> localeColumns = FindLocaleColumnsPlain(header);
            if (keyIndex < 0 || localeColumns.Count == 0) return;

            string csvFile = Path.GetFileName(csvPath);

            for (int i = 1; i < rows.Count; i++)
            {
                string key = ImportHelpers.GetField(rows[i], keyIndex).Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!string.IsNullOrWhiteSpace(keyPrefix) &&
                    !key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!entries.TryGetValue(key, out LocalizationEntry entry))
                {
                    entry = new LocalizationEntry(key);
                    entries[key] = entry;
                }

                foreach (var lc in localeColumns)
                {
                    string value = ImportHelpers.GetField(rows[i], lc.Value);
                    entry.SetIfNonEmpty(lc.Key, value, csvFile);
                }
            }
        }

        public static void Import(Dictionary<string, LocalizationEntry> entries, string tableName,
            string tablesFolder, string localesFolder)
        {
            if (entries == null || entries.Count == 0) return;

            HashSet<string> localeCodes = new(StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries.Values)
                foreach (string code in e.LocaleCodes)
                    localeCodes.Add(code);

            if (localeCodes.Count == 0)
            {
                Debug.LogWarning($"[SPJ] No localization columns found. Skipping localization import for '{tableName}'.");
                return;
            }

            List<Locale> locales = new();
            foreach (string code in localeCodes)
                locales.Add(EnsureLocale(code, localesFolder));

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    tableName, tablesFolder, locales);
            }

            Dictionary<string, StringTable> tablesByCode = new(StringComparer.OrdinalIgnoreCase);
            foreach (Locale locale in locales)
            {
                StringTable table = collection.GetTable(locale.Identifier) as StringTable;
                if (table == null)
                    table = collection.AddNewTable(locale.Identifier) as StringTable;

                if (table == null)
                {
                    Debug.LogError($"[SPJ] Failed to create string table for locale '{locale.Identifier.Code}'.");
                    continue;
                }

                tablesByCode[locale.Identifier.Code] = table;
            }

            Dictionary<string, bool> tableDirty = new(StringComparer.OrdinalIgnoreCase);
            bool sharedDirty = false;

            foreach (var kvp in entries)
            {
                string key = kvp.Key;
                LocalizationEntry entry = kvp.Value;

                foreach (string code in localeCodes)
                {
                    if (!tablesByCode.TryGetValue(code, out StringTable table)) continue;

                    string value = entry.TryGetValue(code, out string v) ? v : string.Empty;

                    StringTableEntry tableEntry = table.GetEntry(key);
                    if (tableEntry == null)
                    {
                        if (table.SharedData.GetEntry(key) == null)
                            sharedDirty = true;

                        table.AddEntry(key, value);
                        tableDirty[code] = true;
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(value) && tableEntry.LocalizedValue != value)
                    {
                        tableEntry.Value = value;
                        tableDirty[code] = true;
                    }
                }
            }

            foreach (var kvp in tableDirty)
            {
                if (kvp.Value && tablesByCode.TryGetValue(kvp.Key, out StringTable table))
                    EditorUtility.SetDirty(table);
            }

            if (sharedDirty)
                EditorUtility.SetDirty(collection.SharedData);
        }

        private static Locale EnsureLocale(string code, string localesFolder)
        {
            Locale existing = LocalizationEditorSettings.GetLocale(code);
            if (existing != null) return existing;

            string fileSafeCode = MakeFileSafe(code);
            string assetPath = $"{localesFolder}/{fileSafeCode}.asset";
            Locale locale = AssetDatabase.LoadAssetAtPath<Locale>(assetPath);
            if (locale == null)
            {
                locale = Locale.CreateLocale(code);
                locale.name = fileSafeCode;
                AssetDatabase.CreateAsset(locale, assetPath);
            }

            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static Dictionary<string, int> FindLocaleColumns(string[] header)
        {
            Dictionary<string, int> result = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Length; i++)
            {
                string raw = (header[i] ?? string.Empty).Trim();
                if (raw.Length < LOCALIZATION_PREFIX.Length)
                    continue;

                if (!raw.StartsWith(LOCALIZATION_PREFIX, StringComparison.OrdinalIgnoreCase))
                    continue;

                string code = raw.Substring(LOCALIZATION_PREFIX.Length).Trim();
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                if (!result.ContainsKey(code))
                    result[code] = i;
            }
            return result;
        }

        private static Dictionary<string, int> FindLocaleColumnsPlain(string[] header)
        {
            Dictionary<string, int> result = new(StringComparer.OrdinalIgnoreCase);
            if (header == null) return result;

            HashSet<string> excluded = new(StringComparer.OrdinalIgnoreCase)
            {
                "key", "name_key", "screen_id", "section_id", "element_id",
                "name_id", "notes", "source_sheet", "source_row"
            };

            for (int i = 0; i < header.Length; i++)
            {
                string raw = (header[i] ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(raw) || excluded.Contains(raw))
                    continue;
                if (!result.ContainsKey(raw))
                    result[raw] = i;
            }

            return result;
        }

        private static string MakeFileSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Locale";

            StringBuilder sb = new(value.Length);
            foreach (char c in value)
            {
                if (c == '/' || c == '\\' || c == ':' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|')
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public sealed class LocalizationEntry
        {
            private readonly string _key;
            private readonly Dictionary<string, string> _valuesByLocale = new(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, string> _sourcesByLocale = new(StringComparer.OrdinalIgnoreCase);

            public IEnumerable<string> LocaleCodes => _valuesByLocale.Keys;

            public LocalizationEntry(string key) { _key = key; }

            public bool TryGetValue(string localeCode, out string value)
            {
                return _valuesByLocale.TryGetValue(localeCode, out value);
            }

            public void Set(string localeCode, string value)
            {
                if (!string.IsNullOrWhiteSpace(localeCode) && !string.IsNullOrWhiteSpace(value))
                    _valuesByLocale[localeCode] = value;
            }

            public void SetIfNonEmpty(string localeCode, string value, string source)
            {
                if (string.IsNullOrWhiteSpace(localeCode) || string.IsNullOrWhiteSpace(value))
                    return;

                if (_valuesByLocale.TryGetValue(localeCode, out string existing) && existing != value)
                {
                    string prevSource = _sourcesByLocale.TryGetValue(localeCode, out string s) ? s : "?";
                    Debug.LogWarning($"[SPJ] Localization conflict for key='{_key}', locale='{localeCode}': '{existing}' ({prevSource}) -> '{value}' ({source})");
                }

                _valuesByLocale[localeCode] = value;
                _sourcesByLocale[localeCode] = source;
            }
        }
    }
}
