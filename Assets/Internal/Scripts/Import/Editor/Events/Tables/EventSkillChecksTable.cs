using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventSkillChecksTable
    {
        public struct SkillCheckRaw
        {
            public int ChoiceIndex;
            public string SkillType;
            public float BaseChance;
            public string FailResultKey;
        }

        public static Dictionary<string, List<SkillCheckRaw>> Read(
            string csvFile = "event_skill_checks.csv")
        {
            Dictionary<string, List<SkillCheckRaw>> map = new(StringComparer.Ordinal);
            var rows = CsvReader.ReadFileSafe(CsvPath(csvFile));
            if (rows == null) return map;

            string[] header = rows[0];
            int eventIdIndex = FindColumnIndex(header, "event_id");
            int choiceIndexIndex = FindColumnIndex(header, "choice_index");
            int skillTypeIndex = FindColumnIndex(header, "skill_type");
            int baseChanceIndex = FindColumnIndex(header, "base_chance");
            int failResultIndex = FindColumnIndex(header, "fail_result_key");

            if (eventIdIndex < 0 || choiceIndexIndex < 0 || skillTypeIndex < 0 || baseChanceIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in event_skill_checks.csv");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string eventId = GetField(rows[i], eventIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(eventId)) continue;

                TryParseInt(GetField(rows[i], choiceIndexIndex), out int choiceIndex);
                string skillType = GetField(rows[i], skillTypeIndex).Trim();
                TryParseFloat(GetField(rows[i], baseChanceIndex), out float baseChance);
                string failResultKey = GetField(rows[i], failResultIndex).Trim();

                if (string.IsNullOrWhiteSpace(skillType)) continue;

                map.GetOrCreateList(eventId).Add(new SkillCheckRaw
                {
                    ChoiceIndex = choiceIndex,
                    SkillType = skillType,
                    BaseChance = baseChance,
                    FailResultKey = failResultKey
                });
            }

            return map;
        }
    }
}
