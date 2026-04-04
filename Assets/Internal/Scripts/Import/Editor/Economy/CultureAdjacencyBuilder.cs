using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy
{
    public static class CultureAdjacencyBuilder
    {
        private const string ASSET_PATH = DATABASES_FOLDER + "/CultureAdjacencyData.asset";

        [MenuItem("SPJ/Import/Build Culture Adjacency")]
        public static void Build()
        {
            if (IsCompiling()) return;

            string dbPath = $"{DATABASES_FOLDER}/EconomyDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<EconomyDatabase>(dbPath);
            if (db == null)
            {
                Debug.LogError("[SPJ] EconomyDatabase not found. Run Economy import first.");
                return;
            }

            var nodeToCity = new Dictionary<string, CityData>();
            foreach (var city in db.Cities)
            {
                if (!string.IsNullOrEmpty(city.NodeId))
                    nodeToCity[city.NodeId] = city;
            }

            var pairs = new List<CultureAdjacencyData.CulturePair>();
            var seen = new HashSet<(CultureId, CultureId)>();

            var roadGuids = AssetDatabase.FindAssets("t:RoadData");
            foreach (var guid in roadGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var road = AssetDatabase.LoadAssetAtPath<RoadData>(path);
                if (road == null) continue;

                if (!nodeToCity.TryGetValue(road.StartNodeId, out var cityA)) continue;
                if (!nodeToCity.TryGetValue(road.EndNodeId, out var cityB)) continue;

                CultureId cA = cityA.PrimaryCulture;
                CultureId cB = cityB.PrimaryCulture;

                if (cA == cB || cA == CultureId.None || cB == CultureId.None) continue;

                var key = cA < cB ? (cA, cB) : (cB, cA);
                if (seen.Add(key))
                    pairs.Add(new CultureAdjacencyData.CulturePair { A = key.Item1, B = key.Item2 });
            }

            EnsureAssetFolder(DATABASES_FOLDER);
            var asset = LoadOrCreateAsset<CultureAdjacencyData>(ASSET_PATH);
            asset.SetAdjacencies(pairs);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SPJ] CultureAdjacency built: {pairs.Count} unique pairs.");
        }
    }
}
