using System;
using System.Collections.Generic;
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

        [MenuItem("SPJ/Import/Events/Import Data")]
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

                // 2. Ensure folders
                EnsureAssetFolder(EVENTS_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);

                // 3. Build lookups
                var typeNameKeys = EventTypesTable.Read();
                var choices = EventChoicesTable.Read();
                EventConditionsTable.Read(out var eventConditions, out var choiceConditions);
                var outcomes = EventOutcomesTable.Read();

                // 4. Localization
                var locEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "event.", locEntries);
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "event_type.", locEntries);
                LocalizationImporter.Import(
                    locEntries, LOCALIZATION_TABLE_NAME, LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                // 5. Import events + database
                var events = EventsTable.Import(
                    typeNameKeys, choices, eventConditions, choiceConditions, outcomes, LOCALIZATION_TABLE_NAME);
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
