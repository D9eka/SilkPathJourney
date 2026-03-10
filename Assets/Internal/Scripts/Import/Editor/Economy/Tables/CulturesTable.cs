using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Player.Languages.Generated;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CulturesTable
    {
        public static (Dictionary<string, CultureId> CultureMap, List<EconomyDatabase.CultureLanguageMapping> CultureLanguages)
            Read()
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("cultures.csv"));
            var map = BuildEnumMap<CultureId>(rows, "cultures.csv", "culture_id", "enum_name");

            var cultureLanguages = new List<EconomyDatabase.CultureLanguageMapping>();
            if (rows.Count == 0) return (map, cultureLanguages);

            string[] header = rows[0];
            int cultureCol = FindColumnIndex(header, "culture_id");
            int langCol = FindColumnIndex(header, "language_id");
            if (cultureCol < 0 || langCol < 0) return (map, cultureLanguages);

            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                string cultureRaw = GetField(row, cultureCol).Trim();
                string langRaw = GetField(row, langCol).Trim();
                if (string.IsNullOrEmpty(cultureRaw) || string.IsNullOrEmpty(langRaw)) continue;

                if (!map.TryGetValue(cultureRaw, out CultureId cultureId)) continue;
                if (!Enum.TryParse(langRaw, true, out LanguageType langType)) continue;

                cultureLanguages.Add(new EconomyDatabase.CultureLanguageMapping
                {
                    Culture = cultureId,
                    Language = langType
                });
            }

            return (map, cultureLanguages);
        }
    }
}
