using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Import.Editor.Core;
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
            Dictionary<string, Dictionary<int, List<EventOutcomeEntry>>> outcomes,
            string locTableName)
        {
            string csvPath = CsvPath("events.csv");
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
                string descKey = descKeyIndex >= 0 ? GetField(rows[i], descKeyIndex).Trim() : "";
                string imageName = imageIndex >= 0 ? GetField(rows[i], imageIndex).Trim() : "";
                TryParseFloat(weightIndex >= 0 ? GetField(rows[i], weightIndex) : "1", out float weight);
                if (weight <= 0f) weight = 1f;

                bool isMinor = false;
                if (isMinorIndex >= 0)
                {
                    TryParseInt(GetField(rows[i], isMinorIndex), out int isMinorInt);
                    isMinor = isMinorInt != 0;
                }

                LocalizedString nameLS = MakeLocalizedString(nameKey, locTableName);
                LocalizedString descLS = MakeLocalizedString(descKey, locTableName);

                LocalizedString eventTypeLS = new LocalizedString();
                if (typeNameKeys.TryGetValue(typeId, out string typeNameKey))
                    eventTypeLS = MakeLocalizedString(typeNameKey, locTableName);
                else
                    Debug.LogWarning($"[SPJ] Unknown event_type_id '{typeId}' in events.csv (row {i + 1})");

                Sprite image = LoadEventSprite(imageName);

                List<EventChoice> builtChoices = BuildChoicesForEvent(
                    id, choices, choiceConditions, outcomes, locTableName);

                List<EventOutcomeEntry> autoOutcomes = ExtractAutoOutcomes(id, outcomes);

                eventConditions.TryGetValue(id, out List<EventCondition> conditions);

                EventData asset = LoadOrCreateAsset<EventData>(EVENTS_FOLDER, id);
                asset.ApplyImport(id, nameLS, eventTypeLS, descLS, image, isMinor, builtChoices, autoOutcomes, conditions, weight);
                EditorUtility.SetDirty(asset);
                events.Add(asset);
            }

            return events;
        }

        private static List<EventChoice> BuildChoicesForEvent(
            string eventId,
            Dictionary<string, List<EventChoicesTable.ChoiceRaw>> choicesByEvent,
            Dictionary<string, Dictionary<int, List<EventCondition>>> choiceConditions,
            Dictionary<string, Dictionary<int, List<EventOutcomeEntry>>> choiceOutcomes,
            string locTableName)
        {
            List<EventChoice> result = new();

            if (!choicesByEvent.TryGetValue(eventId, out List<EventChoicesTable.ChoiceRaw> rawChoices))
                return result;

            choiceConditions.TryGetValue(eventId, out Dictionary<int, List<EventCondition>> condByIdx);
            choiceOutcomes.TryGetValue(eventId, out Dictionary<int, List<EventOutcomeEntry>> outByIdx);

            foreach (var raw in rawChoices)
            {
                LocalizedString textLS = MakeLocalizedString(raw.NameKey, locTableName);
                LocalizedString resultLS = string.IsNullOrEmpty(raw.ResultKey)
                    ? new LocalizedString()
                    : MakeLocalizedString(raw.ResultKey, locTableName);

                List<EventCondition> conds = null;
                condByIdx?.TryGetValue(raw.Index, out conds);

                List<EventOutcomeEntry> outs = null;
                outByIdx?.TryGetValue(raw.Index, out outs);

                result.Add(new EventChoice(textLS, resultLS, conds, outs));
            }

            return result;
        }

        private static List<EventOutcomeEntry> ExtractAutoOutcomes(
            string eventId,
            Dictionary<string, Dictionary<int, List<EventOutcomeEntry>>> outcomes)
        {
            if (outcomes == null) return null;
            if (!outcomes.TryGetValue(eventId, out var byChoice)) return null;
            byChoice.TryGetValue(0, out List<EventOutcomeEntry> autoOutcomes);
            return autoOutcomes;
        }

        private static Sprite LoadEventSprite(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            string path = $"{SPRITES_FOLDER}/{imageName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"[SPJ] Event sprite not found: {path}");
            return sprite;
        }
    }
}
