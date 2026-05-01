using System;
using Internal.Scripts.Import.Editor.Achievement;
using Internal.Scripts.Import.Editor.Camp;
using Internal.Scripts.Import.Editor.Caravan;
using Internal.Scripts.Import.Editor.Economy;
using Internal.Scripts.Import.Editor.Events;
using Internal.Scripts.Import.Editor.Languages;
using Internal.Scripts.Import.Editor.Npc;
using Internal.Scripts.Import.Editor.Player;
using Internal.Scripts.Import.Editor.Quests;
using Internal.Scripts.Import.Editor.Theme;
using Internal.Scripts.Import.Editor.Trader;
using Internal.Scripts.Import.Editor.Unlock;
using Internal.Scripts.World.Roads.Editor;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class MasterImporter
    {
        [MenuItem("SPJ/Master/Import/Generate")]
        public static void Generate()
        {
            if (ImportHelpers.IsCompiling()) return;
            try
            {
                EconomyImporter.Generate();
                EventImporter.Generate();
                QuestImporter.Generate();
                CaravanImporter.Generate();
                ThemeImporter.Generate();
                LanguageTypeGenerator.Generate();
                Debug.Log("[SPJ] All enums generated.");
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        [MenuItem("SPJ/Master/Import/Import")]
        public static void Import()
        {
            if (ImportHelpers.IsCompiling()) return;
            try
            {
                EconomyImporter.Import();
                EventImporter.Import();
                QuestImporter.Import();
                CaravanImporter.Import();
                CampImporter.Import();
                ThemeImporter.Import();
                NpcImporter.Import();
                TraderImporter.Import();
                BackgroundImporter.Import();
                RoadRegionAutoFiller.AutoFillRegions();
                UnlockImporter.Import();
                AchievementImporter.Import();
                LocalizationMasterImporter.Import();
                CultureAdjacencyBuilder.Build();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] All imports finished.");
            }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}
