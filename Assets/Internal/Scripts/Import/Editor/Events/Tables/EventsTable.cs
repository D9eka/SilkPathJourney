using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Player.Skills;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventsTable
    {
        private const string EVENTS_FOLDER = GENERATED_DATA_FOLDER + "/Events";
        private const string SPRITES_FOLDER = "Assets/Internal/Sprites/UI/Window/Event";

        public static List<EventData> Import(
            Dictionary<string, string> typeNameKeys,
            Dictionary<string, List<EventChoicesTable.ChoiceRaw>> choices,
            Dictionary<string, List<EventCondition>> eventConditions,
            Dictionary<string, Dictionary<int, List<EventCondition>>> choiceConditions,
            Dictionary<string, Dictionary<int, EventOutcomesTable.OutcomeBranch>> outcomes,
            Dictionary<string, List<EventSkillChecksTable.SkillCheckRaw>> skillChecksRaw,
            string locTableName,
            string csvFile = "events.csv")
        {
            string csvPath = CsvPath(csvFile);
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<EventData> events = new();

            if (rows.Count == 0) return events;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "event_id");
            int typeIdIndex = FindColumnIndex(header, "event_type_id");
            int nameKeyIndex = FindColumnIndex(header, "name_key");
            int descKeyIndex = FindColumnIndex(header, "description_key");
            int imageIndex = FindColumnIndex(header, "image_name");
            int weightIndex = FindColumnIndex(header, "weight");
            int isMinorIndex = FindColumnIndex(header, "is_minor");

            if (idIndex < 0 || typeIdIndex < 0 || nameKeyIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in events.csv");
                return events;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id)) continue;

                string typeId = GetField(rows[i], typeIdIndex).Trim();
                string nameKey = GetField(rows[i], nameKeyIndex).Trim();
                string descKey = GetField(rows[i], descKeyIndex).Trim();
                string weightStr = GetField(rows[i], weightIndex);
                TryParseFloat(string.IsNullOrEmpty(weightStr) ? "1" : weightStr, out float weight);

                bool isMinor = false;
                if (isMinorIndex >= 0)
                {
                    TryParseInt(GetField(rows[i], isMinorIndex), out int isMinorInt);
                    isMinor = isMinorInt != 0;
                }

                LocalizedString nameLS = MakeLocalizedString(nameKey, locTableName);
                LocalizedString descLS = MakeLocalizedString(descKey, locTableName);

                string typeNameKey = TryLookup(typeNameKeys, typeId, "", csvFile, i + 1, "event_type_id");
                LocalizedString eventTypeLS = MakeLocalizedString(typeNameKey, locTableName);

                Sprite image = LoadSprite(SPRITES_FOLDER, rows[i], imageIndex, "Event");

                List<EventChoice> builtChoices = BuildChoicesForEvent(
                    id, choices, choiceConditions, outcomes, locTableName);

                List<EventOutcomeEntry> autoOutcomes = ExtractAutoOutcomes(id, outcomes);
                List<SkillCheckData> skillChecks = BuildSkillChecks(id, skillChecksRaw, outcomes, locTableName);

                eventConditions.TryGetValue(id, out List<EventCondition> conditions);

                EventData asset = LoadOrCreateAsset<EventData>(EVENTS_FOLDER, id);
                asset.ApplyImport(id, nameLS, eventTypeLS, descLS, image, isMinor, builtChoices, autoOutcomes, conditions, weight, skillChecks);
                EditorUtility.SetDirty(asset);
                events.Add(asset);
            }

            return events;
        }

        private static List<EventChoice> BuildChoicesForEvent(
            string eventId,
            Dictionary<string, List<EventChoicesTable.ChoiceRaw>> choicesByEvent,
            Dictionary<string, Dictionary<int, List<EventCondition>>> choiceConditions,
            Dictionary<string, Dictionary<int, EventOutcomesTable.OutcomeBranch>> choiceOutcomes,
            string locTableName)
        {
            List<EventChoice> result = new();

            if (!choicesByEvent.TryGetValue(eventId, out List<EventChoicesTable.ChoiceRaw> rawChoices))
                return result;

            choiceConditions.TryGetValue(eventId, out Dictionary<int, List<EventCondition>> condByIdx);
            choiceOutcomes.TryGetValue(eventId, out Dictionary<int, EventOutcomesTable.OutcomeBranch> outByIdx);

            foreach (var raw in rawChoices)
            {
                LocalizedString textLS = MakeLocalizedString(raw.NameKey, locTableName);
                LocalizedString resultLS = string.IsNullOrEmpty(raw.ResultKey)
                    ? new LocalizedString()
                    : MakeLocalizedString(raw.ResultKey, locTableName);

                List<EventCondition> conds = null;
                condByIdx?.TryGetValue(raw.Index, out conds);

                List<EventOutcomeEntry> outs = null;
                if (outByIdx != null && outByIdx.TryGetValue(raw.Index, out EventOutcomesTable.OutcomeBranch branch))
                    outs = branch.Success;

                result.Add(new EventChoice(textLS, resultLS, conds, outs));
            }

            return result;
        }

        private static List<EventOutcomeEntry> ExtractAutoOutcomes(
            string eventId,
            Dictionary<string, Dictionary<int, EventOutcomesTable.OutcomeBranch>> outcomes)
        {
            if (outcomes == null) return null;
            if (!outcomes.TryGetValue(eventId, out var byChoice)) return null;
            if (!byChoice.TryGetValue(0, out EventOutcomesTable.OutcomeBranch branch)) return null;
            return branch.Success;
        }

        private static List<SkillCheckData> BuildSkillChecks(
            string eventId,
            Dictionary<string, List<EventSkillChecksTable.SkillCheckRaw>> skillChecksRaw,
            Dictionary<string, Dictionary<int, EventOutcomesTable.OutcomeBranch>> outcomes,
            string locTableName)
        {
            if (skillChecksRaw == null || !skillChecksRaw.TryGetValue(eventId, out var rawList))
                return null;

            outcomes.TryGetValue(eventId, out var outcomeBranches);

            List<SkillCheckData> result = new();
            foreach (var raw in rawList)
            {
                if (!Enum.TryParse(ToPascalCase(raw.SkillType), out SkillType skillType) || skillType == SkillType.None)
                {
                    Debug.LogWarning($"[SPJ] Unknown skill type '{raw.SkillType}' for event '{eventId}'");
                    continue;
                }

                LocalizedString failResultText = string.IsNullOrEmpty(raw.FailResultKey)
                    ? new LocalizedString()
                    : MakeLocalizedString(raw.FailResultKey, locTableName);

                List<EventOutcomeEntry> failOutcomes = null;
                if (outcomeBranches != null &&
                    outcomeBranches.TryGetValue(raw.ChoiceIndex, out EventOutcomesTable.OutcomeBranch branch))
                    failOutcomes = branch.Fail;

                result.Add(new SkillCheckData(raw.ChoiceIndex, skillType, raw.BaseChance, failResultText, failOutcomes));
            }

            return result.Count > 0 ? result : null;
        }

    }
}
