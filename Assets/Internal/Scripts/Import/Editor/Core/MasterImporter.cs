using System;
using Internal.Scripts.Import.Editor.Economy;
using Internal.Scripts.Import.Editor.Events;
using Internal.Scripts.Import.Editor.Theme;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class MasterImporter
    {
        [MenuItem("SPJ/Import/All")]
        public static void ImportAll()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Cannot import while Unity is compiling.");
                return;
            }

            try
            {
                EconomyImporter.ImportAll();
                EventImporter.ImportAll();
                ThemeImporter.ImportColors();
                ThemeImporter.ImportLocalization();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] All imports finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
