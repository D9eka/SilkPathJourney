using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class EnumGenerator
    {
        public static bool GenerateFromSpec(EnumSpec spec)
        {
            bool changed = GenerateEnum(spec);
            if (changed)
                AssetDatabase.Refresh();
            return changed;
        }

        private static bool GenerateEnum(EnumSpec spec)
        {
            if (!File.Exists(spec.SourceCsvPath))
            {
                Debug.LogError($"[SPJ] CSV not found: {spec.SourceCsvPath}");
                return false;
            }

            List<string[]> rows = CsvReader.ReadFile(spec.SourceCsvPath);
            if (rows.Count == 0)
            {
                Debug.LogError($"[SPJ] CSV empty: {spec.SourceCsvPath}");
                return false;
            }

            string[] header = rows[0];
            int idIndex = ImportHelpers.FindColumnIndex(header, spec.IdColumnName);
            int enumIndex = ImportHelpers.FindColumnIndex(header, spec.EnumNameColumnName);
            if (idIndex < 0 || enumIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing required columns in {spec.SourceCsvPath}");
                return false;
            }

            List<EnumEntry> entries = new();
            HashSet<string> ids = new(StringComparer.Ordinal);
            HashSet<string> names = new(StringComparer.Ordinal);

            for (int i = 1; i < rows.Count; i++)
            {
                string id = ImportHelpers.GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!ids.Add(id))
                {
                    Debug.LogError($"[SPJ] Duplicate id '{id}' in {spec.SourceCsvPath} (row {i + 1})");
                    return false;
                }

                string rawName = ImportHelpers.GetField(rows[i], enumIndex).Trim();
                string enumName = string.IsNullOrWhiteSpace(rawName) ? ImportHelpers.ToPascalCase(id) : rawName;

                if (!ImportHelpers.IsValidIdentifier(enumName))
                {
                    Debug.LogError($"[SPJ] Invalid enum name '{enumName}' in {spec.SourceCsvPath} (row {i + 1})");
                    return false;
                }

                if (string.Equals(enumName, spec.ZeroValueName, StringComparison.Ordinal))
                {
                    Debug.LogError($"[SPJ] Enum name '{spec.ZeroValueName}' is reserved in {spec.SourceCsvPath} (row {i + 1})");
                    return false;
                }

                if (!names.Add(enumName))
                {
                    Debug.LogError($"[SPJ] Duplicate enum name '{enumName}' in {spec.SourceCsvPath} (row {i + 1})");
                    return false;
                }

                entries.Add(new EnumEntry(id, enumName));
            }

            string content = BuildEnumSource(spec.EnumName, entries, spec.ZeroValueName, spec.Namespace);
            ImportHelpers.EnsureDirectory(spec.OutputPath);
            return ImportHelpers.WriteIfChanged(spec.OutputPath, content);
        }

        private static string BuildEnumSource(string enumName, List<EnumEntry> entries, string zeroValueName, string ns)
        {
            StringBuilder sb = new();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    public enum {enumName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        {zeroValueName} = 0,");

            int value = 1;
            foreach (EnumEntry entry in entries)
            {
                sb.AppendLine($"        {entry.Name} = {value},");
                value++;
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public readonly struct EnumSpec
        {
            public string EnumName { get; }
            public string SourceCsvPath { get; }
            public string IdColumnName { get; }
            public string EnumNameColumnName { get; }
            public string OutputPath { get; }
            public string ZeroValueName { get; }
            public string Namespace { get; }

            public EnumSpec(string enumName, string sourceCsvPath, string idColumnName, string enumNameColumnName, string outputPath, string zeroValueName, string ns)
            {
                EnumName = enumName;
                SourceCsvPath = sourceCsvPath;
                IdColumnName = idColumnName;
                EnumNameColumnName = enumNameColumnName;
                OutputPath = outputPath;
                ZeroValueName = zeroValueName;
                Namespace = ns;
            }
        }

        private readonly struct EnumEntry
        {
            public string Id { get; }
            public string Name { get; }

            public EnumEntry(string id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}
