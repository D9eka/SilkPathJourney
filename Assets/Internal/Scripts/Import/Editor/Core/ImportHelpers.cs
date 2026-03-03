using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class ImportHelpers
    {
        public static bool IsCompiling()
        {
            if (!EditorApplication.isCompiling)
                return false;
            Debug.LogWarning("[SPJ] Cannot import while Unity is compiling.");
            return true;
        }

        public const string CSV_FOLDER = "Assets/Internal/Data/__source_csv";
        public const string GENERATED_DATA_FOLDER = "Assets/Internal/Data/Generated";
        public const string DATABASES_FOLDER = GENERATED_DATA_FOLDER + "/Databases";
        public const string LOCALIZATION_FOLDER = GENERATED_DATA_FOLDER + "/Localization";
        public const string LOCALIZATION_LOCALES_FOLDER = LOCALIZATION_FOLDER + "/Locales";
        public const string LOCALIZATION_TABLES_FOLDER = LOCALIZATION_FOLDER + "/StringTables";

        public static string CsvPath(string csvFile)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, csvFile);
        }

        public static int FindColumnIndex(string[] header, string columnName)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (string.Equals(header[i].Trim(), columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        public static string GetField(string[] row, int index)
        {
            if (index < 0 || index >= row.Length)
                return string.Empty;
            return row[index] ?? string.Empty;
        }

        public static bool TryParseFloat(string value, out float result)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string trimmed = value.Trim();
            if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(trimmed, "0", StringComparison.OrdinalIgnoreCase))
                return false;

            return bool.TryParse(trimmed, out bool parsed) && parsed;
        }

        public static string ToPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder sb = new StringBuilder(value.Length);
            bool nextUpper = true;
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(nextUpper ? char.ToUpperInvariant(c) : c);
                    nextUpper = false;
                }
                else
                {
                    nextUpper = true;
                }
            }
            return sb.ToString();
        }

        public static void EnsureAssetFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        public static T LoadOrCreateAsset<T>(string folder, string id) where T : ScriptableObject
        {
            string assetName = ToPascalCase(id);
            if (string.IsNullOrWhiteSpace(assetName))
                assetName = id;

            string newPath = $"{folder}/{assetName}.asset";
            string legacyPath = $"{folder}/{id}.asset";

            T asset = AssetDatabase.LoadAssetAtPath<T>(newPath);
            if (asset != null) return asset;

            T legacy = AssetDatabase.LoadAssetAtPath<T>(legacyPath);
            if (legacy != null)
            {
                string moveResult = AssetDatabase.MoveAsset(legacyPath, newPath);
                if (!string.IsNullOrEmpty(moveResult))
                {
                    Debug.LogWarning($"[SPJ] Failed to move asset '{legacyPath}' -> '{newPath}': {moveResult}");
                    return legacy;
                }

                asset = AssetDatabase.LoadAssetAtPath<T>(newPath);
                if (asset != null) return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, newPath);
            return asset;
        }

        public static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        public static LocalizedString MakeLocalizedString(string key, string tableName)
        {
            if (string.IsNullOrWhiteSpace(key))
                return new LocalizedString();
            return new LocalizedString(tableName, key);
        }

        public static Dictionary<string, TEnum> BuildEnumMap<TEnum>(string csvFile, string idColumn, string enumNameColumn)
            where TEnum : struct, Enum
        {
            string csvPath = CsvPath(csvFile);
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            Dictionary<string, TEnum> map = new(StringComparer.Ordinal);

            if (rows.Count == 0)
                return map;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, idColumn);
            int enumIndex = FindColumnIndex(header, enumNameColumn);
            if (idIndex < 0 || enumIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing required columns in {csvFile}");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string rawName = GetField(rows[i], enumIndex).Trim();
                string enumName = string.IsNullOrWhiteSpace(rawName) ? ToPascalCase(id) : rawName;

                if (!Enum.TryParse(enumName, out TEnum value))
                {
                    Debug.LogError($"[SPJ] Enum value '{enumName}' not found for {typeof(TEnum).Name} (row {i + 1})");
                    continue;
                }

                map[id] = value;
            }

            return map;
        }
    }
}
