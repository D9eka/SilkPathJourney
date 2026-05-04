using System;
using Internal.Scripts.Camp;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Camp.Tables;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Camp
{
    public static class CampImporter
    {
        private const string CAMP_FOLDER = "Assets/Internal/Data/Camp";
        private const string DATABASE_PATH = CAMP_FOLDER + "/CampActionDatabase.asset";

        [MenuItem("SPJ/Import/Camp/Import")]
        public static void Import()
        {
            if (IsCompiling()) return;

            try
            {
                EnsureAssetFolder(CAMP_FOLDER);

                var biomeMap = BuildEnumMap<Biome>("biome_palettes.csv", "biome_id", "enum_name");
                var typeMap = BuildEnumMap<CampActionType>("camp_actions.csv", "action_id", "enum_name");

                var sideEffects = CampActionSideEffectsTable.Read();
                var actions = CampActionsTable.Import(typeMap, sideEffects);
                var biomeMods = CampBiomeModifiersTable.Read(biomeMap);

                CampActionDatabase db = LoadOrCreateAsset<CampActionDatabase>(DATABASE_PATH);
                db.ApplyImport(actions, biomeMods);
                EditorUtility.SetDirty(db);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Camp data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
