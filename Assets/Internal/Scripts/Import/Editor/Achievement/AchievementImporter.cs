using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Meta.Achievements;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Achievement
{
    public static class AchievementImporter
    {
        private const string ACHIEVEMENTS_FOLDER = GENERATED_DATA_FOLDER + "/Achievements";

        [MenuItem("SPJ/Import/Achievements/Import")]
        public static void Import()
        {
            if (IsCompiling()) return;

            try
            {
                EnsureAssetFolder(ACHIEVEMENTS_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                var achievements = AchievementsTable.Import();
                UpdateDatabase(achievements);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Achievement data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void UpdateDatabase(List<AchievementData> achievements)
        {
            string assetPath = $"{DATABASES_FOLDER}/AchievementDatabase.asset";
            AchievementDatabase db = LoadOrCreateAsset<AchievementDatabase>(assetPath);
            db.ApplyImport(achievements);
            EditorUtility.SetDirty(db);
        }
    }
}
