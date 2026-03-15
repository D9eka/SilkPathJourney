using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Import.Editor.Events.Generators;
using Internal.Scripts.Import.Editor.Events.Tables;
using Internal.Scripts.Npc.Encounter;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events
{
    public static class EventImporter
    {
        private const string LOCALIZATION_TABLE_NAME = "Events";
        private const string EVENTS_FOLDER = GENERATED_DATA_FOLDER + "/Events";

        [MenuItem("SPJ/Import/Events/Import")]
        public static void ImportAll()
        {
            if (IsCompiling()) return;

            try
            {
                // 1. Generate enums
                EventConditionTypeGenerator.Generate();
                EventOutcomeTypeGenerator.Generate();

                // 1b. Generate road event CSVs from hidden roads
                RoadEventGenerator.Generate();

                // 2. Ensure folders
                EnsureAssetFolder(EVENTS_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                // 3. Build lookups (main CSVs)
                var typeNameKeys = EventTypesTable.Read();
                var choices = EventChoicesTable.Read();
                EventConditionsTable.Read(out var eventConditions, out var choiceConditions);
                var outcomes = EventOutcomesTable.Read();
                var skillChecks = EventSkillChecksTable.Read();

                // 3b. Merge road event data (only if generated files exist)
                bool hasRoadEvents = File.Exists(CsvPath("road_events.csv"));
                if (hasRoadEvents)
                {
                    MergeInto(choices, EventChoicesTable.Read("road_event_choices.csv"));

                    EventConditionsTable.Read(out var roadEC, out var roadCC, "road_event_conditions.csv");
                    MergeInto(eventConditions, roadEC);
                    MergeInto(choiceConditions, roadCC);

                    MergeInto(outcomes, EventOutcomesTable.Read("road_event_outcomes.csv"));
                    MergeInto(skillChecks, EventSkillChecksTable.Read("road_event_skill_checks.csv"));
                }

                // 4. Import events + database
                var events = EventsTable.Import(
                    typeNameKeys, choices, eventConditions, choiceConditions, outcomes,
                    skillChecks, LOCALIZATION_TABLE_NAME);

                // 4b. Road events
                if (hasRoadEvents)
                {
                    var roadEvents = EventsTable.Import(
                        typeNameKeys, choices, eventConditions, choiceConditions, outcomes,
                        skillChecks, LOCALIZATION_TABLE_NAME, "road_events.csv");
                    events.AddRange(roadEvents);
                }

                UpdateEventDatabase(events);
                UpdateNpcEncounterSettings(events);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Event data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void MergeInto<TKey, TVal>(
            Dictionary<TKey, TVal> target, Dictionary<TKey, TVal> source)
        {
            foreach (var kvp in source)
                target[kvp.Key] = kvp.Value;
        }

        private const string NPC_ENCOUNTER_PREFIX = "npc_encounter_";
        private const string NPC_SETTINGS_PATH = DATABASES_FOLDER + "/NpcEncounterSettings.asset";

        private static void UpdateNpcEncounterSettings(List<EventData> allEvents)
        {
            var encounterEvents = allEvents
                .Where(e => e.Id.StartsWith(NPC_ENCOUNTER_PREFIX, StringComparison.Ordinal))
                .ToList();

            if (encounterEvents.Count == 0)
                return;

            NpcEncounterSettings settings = LoadOrCreateAsset<NpcEncounterSettings>(NPC_SETTINGS_PATH);

            settings.EncounterEvents = encounterEvents;
            EditorUtility.SetDirty(settings);
        }

        private static void UpdateEventDatabase(List<EventData> events)
        {
            string assetPath = $"{DATABASES_FOLDER}/EventDatabase.asset";
            EventDatabase db = LoadOrCreateAsset<EventDatabase>(assetPath);

            db.ApplyImport(events);
            EditorUtility.SetDirty(db);
        }
    }
}
