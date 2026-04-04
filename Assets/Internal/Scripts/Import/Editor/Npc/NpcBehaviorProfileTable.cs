using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Npc.Behavior;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Npc
{
    public static class NpcBehaviorProfileTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/NpcBehaviorProfiles";
        private const string NPC_SETTINGS_PATH = "Assets/Internal/Data/NpcSimulationSettings.asset";

        public static void Import()
        {
            string csvPath = CsvPath("npc_behavior_profiles.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);

            if (rows.Count == 0)
            {
                Debug.LogWarning("[SPJ] npc_behavior_profiles.csv is empty or not found.");
                return;
            }

            string[] header = rows[0];
            int archetypeIndex = FindColumnIndex(header, "archetype_id");
            int visitActionsIndex = FindColumnIndex(header, "visit_actions");
            int dayPhasesIndex = FindColumnIndex(header, "day_phases");

            if (archetypeIndex < 0 || visitActionsIndex < 0 || dayPhasesIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in npc_behavior_profiles.csv (expected archetype_id, visit_actions, day_phases)");
                return;
            }

            EnsureAssetFolder(OUTPUT_FOLDER);

            NpcBehaviorProfile defaultProfile = null;
            List<NpcBehaviorProfile> archetypeProfiles = new();

            for (int i = 1; i < rows.Count; i++)
            {
                string archetypeId = GetField(rows[i], archetypeIndex).Trim();
                if (string.IsNullOrWhiteSpace(archetypeId))
                    continue;

                NpcVisitActionType[] visitActions = ParseEnumList<NpcVisitActionType>(GetField(rows[i], visitActionsIndex), archetypeId);
                NpcDayPhaseType[] dayPhases = ParseEnumList<NpcDayPhaseType>(GetField(rows[i], dayPhasesIndex), archetypeId);

                if (string.Equals(archetypeId, "default", StringComparison.OrdinalIgnoreCase))
                {
                    NpcBehaviorProfile asset = LoadOrCreateAsset<NpcBehaviorProfile>(OUTPUT_FOLDER, archetypeId);
                    asset.ApplyImport(default, visitActions, dayPhases);
                    EditorUtility.SetDirty(asset);
                    defaultProfile = asset;
                }
                else
                {
                    string enumName = ToPascalCase(archetypeId);
                    if (!Enum.TryParse(enumName, out NpcArchetype archetype))
                    {
                        Debug.LogWarning($"[SPJ] Unknown NpcArchetype '{archetypeId}' in npc_behavior_profiles.csv (row {i + 1})");
                        continue;
                    }

                    NpcBehaviorProfile asset = LoadOrCreateAsset<NpcBehaviorProfile>(OUTPUT_FOLDER, archetypeId);
                    asset.ApplyImport(archetype, visitActions, dayPhases);
                    EditorUtility.SetDirty(asset);
                    archetypeProfiles.Add(asset);
                }
            }

            var npcSettings = AssetDatabase.LoadAssetAtPath<NpcSimulationSettings>(NPC_SETTINGS_PATH);
            if (npcSettings != null)
            {
                npcSettings.ApplyBehaviorProfilesImport(defaultProfile, archetypeProfiles);
                EditorUtility.SetDirty(npcSettings);
            }
            else
            {
                Debug.LogWarning($"[SPJ] NpcSimulationSettings not found at {NPC_SETTINGS_PATH}");
            }

            Debug.Log($"[SPJ] Imported {archetypeProfiles.Count} archetype behavior profiles + default.");
        }

        private static TEnum[] ParseEnumList<TEnum>(string raw, string context) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<TEnum>();

            string[] parts = raw.Split(',');
            List<TEnum> result = new(parts.Length);
            foreach (string part in parts)
            {
                string name = part.Trim();
                if (string.IsNullOrEmpty(name))
                    continue;
                if (Enum.TryParse(name, out TEnum value))
                    result.Add(value);
                else
                    Debug.LogWarning($"[SPJ] Unknown {typeof(TEnum).Name} value '{name}' for archetype '{context}'");
            }
            return result.ToArray();
        }
    }
}
