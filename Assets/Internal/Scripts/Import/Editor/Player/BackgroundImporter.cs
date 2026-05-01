using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Player.Background;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Player
{
    public static class BackgroundImporter
    {
        private const string BACKGROUNDS_FOLDER = GENERATED_DATA_FOLDER + "/Backgrounds";

        [MenuItem("SPJ/Import/Backgrounds/Import")]
        public static void Import()
        {
            if (IsCompiling()) return;

            try
            {
                EnsureAssetFolder(BACKGROUNDS_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                var backgrounds = BackgroundsTable.Import();
                UpdateDatabase(backgrounds);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Background data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void UpdateDatabase(List<BackgroundData> backgrounds)
        {
            string assetPath = $"{DATABASES_FOLDER}/BackgroundDatabase.asset";
            BackgroundDatabase db = LoadOrCreateAsset<BackgroundDatabase>(assetPath);
            db.ApplyImport(backgrounds);
            EditorUtility.SetDirty(db);
        }
    }
}
