using System;
using Internal.Scripts.Import.Editor.Achievement;
using Internal.Scripts.Import.Editor.Camp;
using Internal.Scripts.Import.Editor.Economy;
using Internal.Scripts.Import.Editor.Events;
using Internal.Scripts.Import.Editor.Languages;
using Internal.Scripts.Import.Editor.Npc;
using Internal.Scripts.Import.Editor.Player;
using Internal.Scripts.Import.Editor.Quests;
using Internal.Scripts.Import.Editor.Theme;
using Internal.Scripts.Import.Editor.Caravan;
using Internal.Scripts.Import.Editor.Trader;
using Internal.Scripts.Import.Editor.Unlock;
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
                QuestImporter.ImportAll();
                CaravanImporter.ImportAll();
                CampImporter.ImportAll();
                ThemeImporter.ImportColors();
                NpcImporter.ImportAll();
                TraderImporter.Import();
                LanguageTypeGenerator.Generate();
                BackgroundImporter.ImportAll();
                UnlockImporter.ImportAll();
                AchievementImporter.ImportAll();
                LocalizationMasterImporter.ImportAll();
                CultureAdjacencyBuilder.Build();

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
