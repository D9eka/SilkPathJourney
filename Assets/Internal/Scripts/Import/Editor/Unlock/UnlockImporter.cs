using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Meta.Unlocks;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Unlock
{
    public static class UnlockImporter
    {
        private const string UNLOCKS_FOLDER = GENERATED_DATA_FOLDER + "/Unlocks";

        [MenuItem("SPJ/Import/Unlocks/Import")]
        public static void Import()
        {
            if (IsCompiling()) return;

            try
            {
                EnsureAssetFolder(UNLOCKS_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                var unlocks = UnlocksTable.Import();
                UpdateDatabase(unlocks);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Unlock data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void UpdateDatabase(List<UnlockData> unlocks)
        {
            string assetPath = $"{DATABASES_FOLDER}/UnlockDatabase.asset";
            UnlockDatabase db = LoadOrCreateAsset<UnlockDatabase>(assetPath);
            db.ApplyImport(unlocks);
            EditorUtility.SetDirty(db);
        }
    }
}
