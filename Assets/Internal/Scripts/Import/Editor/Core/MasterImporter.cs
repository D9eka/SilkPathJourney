using System;
using Internal.Scripts.Import.Editor.Economy;
using Internal.Scripts.Import.Editor.Events;
using Internal.Scripts.Import.Editor.Npc;
using Internal.Scripts.Import.Editor.Theme;
using Internal.Scripts.Import.Editor.Trader;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class MasterImporter
    {
        [MenuItem("SPJ/Import/All")]
        public static void ImportAll()
        {
            if (ImportHelpers.IsCompiling()) return;

            try
            {
                EconomyImporter.ImportAll();
                EventImporter.ImportAll();
                ThemeImporter.ImportColors();
                ThemeImporter.ImportLocalization();
                NpcImporter.ImportNames();
                TraderImporter.Import();

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
