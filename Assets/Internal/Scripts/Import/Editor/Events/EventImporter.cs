using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Import.Editor.Events.Generators;
using Internal.Scripts.Import.Editor.Events.Tables;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events
{
    public static class EventImporter
    {
        private const string LOCALIZATION_TABLE_NAME = "Events";
        private const string EVENTS_FOLDER = GENERATED_DATA_FOLDER + "/Events";

        [MenuItem("SPJ/Import/Events/Import All")]
        public static void ImportAll()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Cannot import while compiling. Please wait and try again.");
                return;
            }

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
                EnsureAssetFolder(LOCALIZATION_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);

                // 3. Build lookups (main CSVs)
                var typeNameKeys = EventTypesTable.Read();
                var choices = EventChoicesTable.Read();
                EventConditionsTable.Read(out var eventConditions, out var choiceConditions);
                var outcomes = EventOutcomesTable.Read();

                // 3b. Merge road event data (only if generated files exist)
                bool hasRoadEvents = File.Exists(CsvPath("road_events.csv"));
                if (hasRoadEvents)
                {
                    MergeInto(choices, EventChoicesTable.Read("road_event_choices.csv"));

                    EventConditionsTable.Read(out var roadEC, out var roadCC, "road_event_conditions.csv");
                    MergeInto(eventConditions, roadEC);
                    MergeNestedInto(choiceConditions, roadCC);

                    MergeNestedInto(outcomes, EventOutcomesTable.Read("road_event_outcomes.csv"));
                }

                // 4. Localization
                var locEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "event.", locEntries);
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "event_type.", locEntries);

                // 4b. Road event localization
                if (hasRoadEvents)
                    LocalizationImporter.CollectFromCsvPlainLocales(
                        CsvPath("road_events_localization.csv"), "key", "event.", locEntries);

                LocalizationImporter.Import(
                    locEntries, LOCALIZATION_TABLE_NAME, LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                // 5. Import events + database
                var events = EventsTable.Import(
                    typeNameKeys, choices, eventConditions, choiceConditions, outcomes, LOCALIZATION_TABLE_NAME);

                // 5b. Road events
                if (hasRoadEvents)
                {
                    var roadEvents = EventsTable.Import(
                        typeNameKeys, choices, eventConditions, choiceConditions, outcomes,
                        LOCALIZATION_TABLE_NAME, "road_events.csv");
                    events.AddRange(roadEvents);
                }

                UpdateEventDatabase(events);

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

        private static void MergeNestedInto<TKey, TInner>(
            Dictionary<TKey, TInner> target, Dictionary<TKey, TInner> source)
        {
            foreach (var kvp in source)
                target[kvp.Key] = kvp.Value;
        }

        private static void UpdateEventDatabase(List<EventData> events)
        {
            string assetPath = $"{DATABASES_FOLDER}/EventDatabase.asset";
            EventDatabase db = AssetDatabase.LoadAssetAtPath<EventDatabase>(assetPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<EventDatabase>();
                AssetDatabase.CreateAsset(db, assetPath);
            }

            db.ApplyImport(events);
            EditorUtility.SetDirty(db);
        }
    }
}
