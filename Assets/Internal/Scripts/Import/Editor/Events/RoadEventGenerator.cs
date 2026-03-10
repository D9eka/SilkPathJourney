using System.Collections.Generic;
using System.IO;
using System.Text;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events
{
    public static class RoadEventGenerator
    {
        private const string TEMPLATE_PREFIX = "template_";
        private const string ROAD_EVENTS_CSV = "road_events.csv";
        private const string ROAD_CHOICES_CSV = "road_event_choices.csv";
        private const string ROAD_CONDITIONS_CSV = "road_event_conditions.csv";
        private const string ROAD_OUTCOMES_CSV = "road_event_outcomes.csv";
        private const string ROAD_LOC_CSV = "road_events_localization.csv";
        private const string ROAD_SKILL_CHECKS_CSV = "road_event_skill_checks.csv";

        private struct TemplateData
        {
            public string EventTypeId;
            public string NameKey, DescKey;
            public List<ChoiceData> Choices;
            public SkillCheckData? SkillCheck;
        }

        private struct ChoiceData
        {
            public int Index;
            public string NameKey, ResultKey;
        }

        private struct SkillCheckData
        {
            public int ChoiceIndex;
            public string SkillType;
            public float BaseChance;
            public string FailResultKey;
        }

        [MenuItem("SPJ/Import/Events/Generate Road Events")]
        public static void Generate()
        {
            var locDict = ReadLocalization();
            var templates = ReadTemplatesFromEvents(locDict);

            if (templates.Count == 0)
            {
                Debug.LogError("[SPJ] No template events found (prefix 'template_') in events.csv");
                return;
            }

            List<RoadData> hiddenRoads = FindHiddenRoads();
            if (hiddenRoads.Count == 0)
            {
                Debug.Log("[SPJ] No hidden roads found. Skipping road event generation.");
                return;
            }

            var events = new StringBuilder();
            var choices = new StringBuilder();
            var conditions = new StringBuilder();
            var outcomes = new StringBuilder();
            var loc = new StringBuilder();
            var skillChecks = new StringBuilder();

            events.AppendLine("event_id,event_type_id,name_key,description_key,image_name,image_prompt,weight,is_minor");
            choices.AppendLine("event_id,choice_index,name_key,result_key");
            conditions.AppendLine("event_id,choice_index,type,param,value");
            outcomes.AppendLine("event_id,choice_index,type,param,value,branch");
            loc.AppendLine("key,source_sheet,source_row,ru,en");
            skillChecks.AppendLine("event_id,choice_index,skill_type,base_chance,fail_result_key");

            templates.TryGetValue("discovery", out List<TemplateData> discoveryList);
            templates.TryGetValue("encounter", out List<TemplateData> encounterList);
            templates.TryGetValue("quest", out List<TemplateData> questList);

            for (int i = 0; i < hiddenRoads.Count; i++)
            {
                RoadData road = hiddenRoads[i];
                string roadId = road.RoadId;
                string nodeId = road.StartNodeId;

                if (discoveryList != null && discoveryList.Count > 0)
                    WriteDiscoveryEvent(discoveryList[i % discoveryList.Count], roadId, nodeId, locDict, events, choices, conditions, outcomes, loc, skillChecks);
                if (encounterList != null && encounterList.Count > 0)
                    WriteEncounterEvent(encounterList[i % encounterList.Count], roadId, nodeId, locDict, events, choices, conditions, outcomes, loc);
                if (questList != null && questList.Count > 0)
                    WriteQuestEvent(questList[i % questList.Count], roadId, nodeId, locDict, events, choices, conditions, outcomes, loc);
            }

            WriteCsv(ROAD_EVENTS_CSV, events);
            WriteCsv(ROAD_CHOICES_CSV, choices);
            WriteCsv(ROAD_CONDITIONS_CSV, conditions);
            WriteCsv(ROAD_OUTCOMES_CSV, outcomes);
            WriteCsv(ROAD_LOC_CSV, loc);
            WriteCsv(ROAD_SKILL_CHECKS_CSV, skillChecks);

            Debug.Log($"[SPJ] Generated road events for {hiddenRoads.Count} hidden road(s).");
        }

        private static List<RoadData> FindHiddenRoads()
        {
            List<RoadData> result = new();
            string[] guids = AssetDatabase.FindAssets("t:RoadData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RoadData data = AssetDatabase.LoadAssetAtPath<RoadData>(path);
                if (data != null && data.IsHidden && !string.IsNullOrEmpty(data.RoadId))
                    result.Add(data);
            }

            result.Sort((a, b) => string.Compare(a.RoadId, b.RoadId, System.StringComparison.Ordinal));
            return result;
        }

        #region Discovery

        private static void WriteDiscoveryEvent(TemplateData t, string roadId, string nodeId,
            Dictionary<string, (string ru, string en)> locDict,
            StringBuilder events, StringBuilder choices, StringBuilder conditions,
            StringBuilder outcomes, StringBuilder loc, StringBuilder skillChecks)
        {
            string eventId = $"road_discover_{roadId}";

            events.AppendLine($"{eventId},discovery,event.{eventId}.name,event.{eventId}.description,,,2,0");

            foreach (var choice in t.Choices)
            {
                choices.AppendLine($"{eventId},{choice.Index},event.{eventId}.choice{choice.Index},event.{eventId}.choice{choice.Index}.result");
                WriteLocFromKey(loc, $"event.{eventId}.choice{choice.Index}", choice.NameKey, locDict);
                WriteLocFromKey(loc, $"event.{eventId}.choice{choice.Index}.result", choice.ResultKey, locDict);
            }

            conditions.AppendLine($"{eventId},,near_node,{nodeId},0");

            outcomes.AppendLine($"{eventId},1,unlock_road,{roadId},0,");
            outcomes.AppendLine($"{eventId},1,add_skill_xp,Survival,3,");
            outcomes.AppendLine($"{eventId},1,danger,,15,fail");

            if (t.SkillCheck.HasValue)
            {
                var sc = t.SkillCheck.Value;
                skillChecks.AppendLine($"{eventId},{sc.ChoiceIndex},{sc.SkillType},{sc.BaseChance},event.{eventId}.choice{sc.ChoiceIndex}.fail");
                WriteLocFromKey(loc, $"event.{eventId}.choice{sc.ChoiceIndex}.fail", sc.FailResultKey, locDict);
            }

            WriteLocFromKey(loc, $"event.{eventId}.name", t.NameKey, locDict);
            WriteLocFromKey(loc, $"event.{eventId}.description", t.DescKey, locDict);
        }

        #endregion

        #region Encounter

        private static void WriteEncounterEvent(TemplateData t, string roadId, string nodeId,
            Dictionary<string, (string ru, string en)> locDict,
            StringBuilder events, StringBuilder choices, StringBuilder conditions,
            StringBuilder outcomes, StringBuilder loc)
        {
            string eventId = $"road_encounter_{roadId}";

            events.AppendLine($"{eventId},encounter,event.{eventId}.name,event.{eventId}.description,,,1,0");

            foreach (var choice in t.Choices)
            {
                choices.AppendLine($"{eventId},{choice.Index},event.{eventId}.choice{choice.Index},event.{eventId}.choice{choice.Index}.result");
                WriteLocFromKey(loc, $"event.{eventId}.choice{choice.Index}", choice.NameKey, locDict);
                WriteLocFromKey(loc, $"event.{eventId}.choice{choice.Index}.result", choice.ResultKey, locDict);
            }

            conditions.AppendLine($"{eventId},,near_node,{nodeId},0");

            outcomes.AppendLine($"{eventId},1,unlock_road,{roadId},0,");
            outcomes.AppendLine($"{eventId},1,add_skill_xp,Survival,3,");

            WriteLocFromKey(loc, $"event.{eventId}.name", t.NameKey, locDict);
            WriteLocFromKey(loc, $"event.{eventId}.description", t.DescKey, locDict);
        }

        #endregion

        #region Quest

        private static void WriteQuestEvent(TemplateData t, string roadId, string nodeId,
            Dictionary<string, (string ru, string en)> locDict,
            StringBuilder events, StringBuilder choices, StringBuilder conditions,
            StringBuilder outcomes, StringBuilder loc)
        {
            string eventId = $"road_quest_{roadId}";

            events.AppendLine($"{eventId},quest,event.{eventId}.name,event.{eventId}.description,,,1,0");

            foreach (var choice in t.Choices)
            {
                choices.AppendLine($"{eventId},{choice.Index},event.{eventId}.choice{choice.Index},event.{eventId}.choice{choice.Index}.result");
                WriteLocFromKey(loc, $"event.{eventId}.choice{choice.Index}", choice.NameKey, locDict);
                WriteLocFromKey(loc, $"event.{eventId}.choice{choice.Index}.result", choice.ResultKey, locDict);
            }

            conditions.AppendLine($"{eventId},,near_node,{nodeId},0");

            outcomes.AppendLine($"{eventId},1,unlock_road,{roadId},0,");
            outcomes.AppendLine($"{eventId},1,add_skill_xp,Survival,3,");

            WriteLocFromKey(loc, $"event.{eventId}.name", t.NameKey, locDict);
            WriteLocFromKey(loc, $"event.{eventId}.description", t.DescKey, locDict);
        }

        #endregion

        #region Read Templates

        private static Dictionary<string, List<TemplateData>> ReadTemplatesFromEvents(
            Dictionary<string, (string ru, string en)> locDict)
        {
            var result = new Dictionary<string, List<TemplateData>>();

            // 1. Read events.csv — filter by template_ prefix
            var eventRows = CsvReader.ReadFile(CsvPath("events.csv"));
            if (eventRows.Count == 0) return result;

            string[] evtHeader = eventRows[0];
            int evtIdIdx = FindColumnIndex(evtHeader, "event_id");
            int evtTypeIdx = FindColumnIndex(evtHeader, "event_type_id");
            int evtNameIdx = FindColumnIndex(evtHeader, "name_key");
            int evtDescIdx = FindColumnIndex(evtHeader, "description_key");

            var templateMap = new Dictionary<string, TemplateData>();

            for (int i = 1; i < eventRows.Count; i++)
            {
                string id = GetField(eventRows[i], evtIdIdx).Trim();
                if (!id.StartsWith(TEMPLATE_PREFIX)) continue;

                var data = new TemplateData
                {
                    EventTypeId = GetField(eventRows[i], evtTypeIdx).Trim(),
                    NameKey = GetField(eventRows[i], evtNameIdx).Trim(),
                    DescKey = GetField(eventRows[i], evtDescIdx).Trim(),
                    Choices = new List<ChoiceData>(),
                };
                templateMap[id] = data;
            }

            // 2. Read event_choices.csv — match template_ IDs
            var choiceRows = CsvReader.ReadFile(CsvPath("event_choices.csv"));
            if (choiceRows.Count > 0)
            {
                string[] chHeader = choiceRows[0];
                int chEventIdx = FindColumnIndex(chHeader, "event_id");
                int chIndexIdx = FindColumnIndex(chHeader, "choice_index");
                int chNameIdx = FindColumnIndex(chHeader, "name_key");
                int chResultIdx = FindColumnIndex(chHeader, "result_key");

                for (int i = 1; i < choiceRows.Count; i++)
                {
                    string eventId = GetField(choiceRows[i], chEventIdx).Trim();
                    if (!templateMap.ContainsKey(eventId)) continue;

                    TryParseInt(GetField(choiceRows[i], chIndexIdx), out int choiceIndex);
                    var choice = new ChoiceData
                    {
                        Index = choiceIndex,
                        NameKey = GetField(choiceRows[i], chNameIdx).Trim(),
                        ResultKey = GetField(choiceRows[i], chResultIdx).Trim(),
                    };

                    var td = templateMap[eventId];
                    td.Choices.Add(choice);
                    templateMap[eventId] = td;
                }
            }

            // 3. Read event_skill_checks.csv — match template_ IDs
            var scRows = CsvReader.ReadFile(CsvPath("event_skill_checks.csv"));
            if (scRows.Count > 0)
            {
                string[] scHeader = scRows[0];
                int scEventIdx = FindColumnIndex(scHeader, "event_id");
                int scChoiceIdx = FindColumnIndex(scHeader, "choice_index");
                int scSkillIdx = FindColumnIndex(scHeader, "skill_type");
                int scChanceIdx = FindColumnIndex(scHeader, "base_chance");
                int scFailIdx = FindColumnIndex(scHeader, "fail_result_key");

                for (int i = 1; i < scRows.Count; i++)
                {
                    string eventId = GetField(scRows[i], scEventIdx).Trim();
                    if (!templateMap.ContainsKey(eventId)) continue;

                    TryParseInt(GetField(scRows[i], scChoiceIdx), out int choiceIndex);
                    TryParseFloat(GetField(scRows[i], scChanceIdx), out float baseChance);

                    var td = templateMap[eventId];
                    td.SkillCheck = new SkillCheckData
                    {
                        ChoiceIndex = choiceIndex,
                        SkillType = GetField(scRows[i], scSkillIdx).Trim(),
                        BaseChance = baseChance,
                        FailResultKey = GetField(scRows[i], scFailIdx).Trim(),
                    };
                    templateMap[eventId] = td;
                }
            }

            // 4. Group by type: template_{type}_{index} → type
            foreach (var kvp in templateMap)
            {
                string suffix = kvp.Key.Substring(TEMPLATE_PREFIX.Length);
                int lastUnderscore = suffix.LastIndexOf('_');
                string type = lastUnderscore > 0 ? suffix.Substring(0, lastUnderscore) : suffix;

                if (!result.TryGetValue(type, out List<TemplateData> list))
                {
                    list = new List<TemplateData>();
                    result[type] = list;
                }

                list.Add(kvp.Value);
            }

            return result;
        }

        private static Dictionary<string, (string ru, string en)> ReadLocalization()
        {
            var dict = new Dictionary<string, (string ru, string en)>();
            var rows = CsvReader.ReadFile(CsvPath("localization.csv"));
            if (rows.Count == 0) return dict;

            string[] header = rows[0];
            int keyIdx = FindColumnIndex(header, "key");
            int ruIdx = FindColumnIndex(header, "ru");
            int enIdx = FindColumnIndex(header, "en");
            if (keyIdx < 0 || ruIdx < 0 || enIdx < 0) return dict;

            for (int i = 1; i < rows.Count; i++)
            {
                string key = GetField(rows[i], keyIdx).Trim();
                if (string.IsNullOrEmpty(key)) continue;
                if (!key.StartsWith("template.")) continue;

                string ru = GetField(rows[i], ruIdx);
                string en = GetField(rows[i], enIdx);
                dict[key] = (ru, en);
            }

            return dict;
        }

        #endregion

        private static void WriteLocFromKey(StringBuilder sb, string targetKey, string sourceKey,
            Dictionary<string, (string ru, string en)> locDict)
        {
            if (locDict.TryGetValue(sourceKey, out var loc))
                sb.AppendLine($"{CsvEscape(targetKey)},road_events,,{CsvEscape(loc.ru)},{CsvEscape(loc.en)}");
            else
                Debug.LogWarning($"[SPJ] Localization key not found: {sourceKey}");
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static void WriteCsv(string fileName, StringBuilder content)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER);
            string filePath = Path.Combine(dir, fileName);
            File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
        }
    }
}
