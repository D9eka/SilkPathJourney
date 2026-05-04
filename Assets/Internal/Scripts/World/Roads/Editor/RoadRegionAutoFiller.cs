using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.World.Roads.Editor
{
    public static class RoadRegionAutoFiller
    {
        [MenuItem("SPJ/Roads/Auto-fill Regions")]
        public static void AutoFillRegions()
        {
            string[] dbGuids = AssetDatabase.FindAssets("t:EconomyDatabase");
            if (dbGuids.Length == 0)
            {
                Debug.LogError("[RoadRegionAutoFiller] EconomyDatabase not found.");
                return;
            }

            string dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
            EconomyDatabase economyDatabase = AssetDatabase.LoadAssetAtPath<EconomyDatabase>(dbPath);

            Dictionary<string, CultureId> nodeToRegion = new Dictionary<string, CultureId>();
            foreach (CityData city in economyDatabase.Cities)
            {
                if (city == null || string.IsNullOrWhiteSpace(city.NodeId))
                    continue;
                nodeToRegion[city.NodeId] = city.PrimaryCulture;
            }

            string[] roadGuids = AssetDatabase.FindAssets("t:RoadData");
            int updated = 0;
            foreach (string guid in roadGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RoadData road = AssetDatabase.LoadAssetAtPath<RoadData>(path);
                if (road == null)
                    continue;

                if (!nodeToRegion.TryGetValue(road.StartNodeId, out CultureId startCulture))
                {
                    Debug.LogWarning($"[RoadRegionAutoFiller] StartNodeId '{road.StartNodeId}' of road '{road.RoadId}' not found in city list. Skipping.");
                    continue;
                }

                road.Region = startCulture;
                EditorUtility.SetDirty(road);
                updated++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[RoadRegionAutoFiller] Updated {updated} of {roadGuids.Length} roads.");
        }
    }
}
