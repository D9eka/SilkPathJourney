using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Import.Editor.Roads.DTO;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Internal.Scripts.Import.Editor.Roads
{
    public static class RoadSceneBuilder
    {
        private const string RoadsRootName = "SPJ_Roads";

        [MenuItem("SPJ/Roads/Build Roads In Current Scene")]
        public static void BuildRoadsInCurrentScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;

            RoadData[] roadDatas = FindAllRoadDataAssets();
            if (roadDatas.Length == 0)
            {
                Debug.Log("[SPJ] No RoadData assets found.");
                return;
            }

            Transform defaultWorldRoot = FindWorldRoot(roadDatas[0]);
            if (defaultWorldRoot == null)
                Debug.LogWarning($"[SPJ] World root '{roadDatas[0].RelativeTo}' not found. Roads will be created at scene root.");

            GameObject roadsRoot = GameObject.Find(RoadsRootName);
            if (roadsRoot == null)
            {
                roadsRoot = new GameObject(RoadsRootName);
                Undo.RegisterCreatedObjectUndo(roadsRoot, "Create SPJ_Roads");
            }

            if (defaultWorldRoot != null && roadsRoot.transform.parent != defaultWorldRoot)
            {
                Undo.RecordObject(roadsRoot.transform, "Parent SPJ_Roads");
                roadsRoot.transform.SetParent(defaultWorldRoot, false);
                roadsRoot.transform.localPosition = Vector3.zero;
                roadsRoot.transform.localRotation = Quaternion.identity;
                roadsRoot.transform.localScale = Vector3.one;
            }

            int created = 0, updated = 0;

            foreach (RoadData rd in roadDatas)
            {
                if (rd == null || string.IsNullOrWhiteSpace(rd.RoadId))
                    continue;

                string goName = $"Road_{rd.RoadId}";
                Transform existing = roadsRoot.transform.Cast<Transform>().FirstOrDefault(t => t.name == goName);

                if (existing == null)
                {
                    GameObject go = new GameObject(goName);
                    Undo.RegisterCreatedObjectUndo(go, "Create Road");
                    go.transform.SetParent(roadsRoot.transform, false);
                    existing = go.transform;
                    created++;
                }

                RoadRuntime rr = existing.GetComponent<RoadRuntime>();
                if (rr == null) rr = Undo.AddComponent<RoadRuntime>(existing.gameObject);

                if (rr.Data != rd)
                {
                    Undo.RecordObject(rr, "Assign RoadData");
                    rr.SetData(rd);
                    updated++;
                }

                Transform wr = FindWorldRoot(rd) ?? defaultWorldRoot;
                rr.SetWorldRoot(wr);
            }

            var validNames = new HashSet<string>(
                roadDatas.Where(r => r != null && !string.IsNullOrWhiteSpace(r.RoadId))
                         .Select(r => $"Road_{r.RoadId}"),
                StringComparer.Ordinal);

            var orphans = roadsRoot.transform.Cast<Transform>()
                .Where(t => t.name.StartsWith("Road_", StringComparison.Ordinal) && !validNames.Contains(t.name))
                .ToList();

            int removedFromScene = 0;
            foreach (Transform t in orphans)
            {
                Undo.DestroyObjectImmediate(t.gameObject);
                removedFromScene++;
            }

            BindCitiesToNodes();
            RoadMaterialPainter.PaintRoadMaterials();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[SPJ] Roads built. Created: {created}, Updated: {updated}, Removed orphans: {removedFromScene}, Total: {roadDatas.Length}");
        }

        private static RoadData[] FindAllRoadDataAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:RoadData");
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<RoadData>(p))
                .Where(a => a != null)
                .ToArray();
        }

        private static Transform FindWorldRoot(RoadData rd)
        {
            if (rd == null || string.IsNullOrWhiteSpace(rd.RelativeTo))
                return null;

            GameObject go = GameObject.Find(rd.RelativeTo);
            return go != null ? go.transform : null;
        }

        private static void BindCitiesToNodes()
        {
            EconomyDatabase db = FindEconomyDatabase();
            if (db == null)
            {
                Debug.LogWarning("[SPJ] EconomyDatabase not found. City-node links were not created.");
                return;
            }

            Dictionary<string, Transform> nodes = ImportHelpers.BuildSceneNodeLookup("[SPJ]");
            if (nodes.Count == 0)
            {
                Debug.LogWarning($"[SPJ] No nodes found with prefix '{NodeIdRules.NodePrefix}'. City-node links were not created.");
                return;
            }

            CityView cityViewPrefab = FindCityViewPrefab();
            Dictionary<string, Biome> nodeBiomes = LoadNodeBiomesFromJson();

            int linked = 0;
            int missing = 0;
            int biomesSet = 0;

            foreach (CityData city in db.Cities)
            {
                if (city == null || string.IsNullOrWhiteSpace(city.NodeId))
                {
                    missing++;
                    continue;
                }

                if (!nodes.TryGetValue(city.NodeId, out Transform node))
                {
                    missing++;
                    continue;
                }

                CityNodeLink link = node.GetComponent<CityNodeLink>();
                if (link == null)
                    link = Undo.AddComponent<CityNodeLink>(node.gameObject);

                Undo.RecordObject(link, "Bind City To Node");
                link.ApplyLink(city);

                if (nodeBiomes.TryGetValue(city.NodeId, out Biome biome))
                {
                    Undo.RecordObject(city, "Set City Biome");
                    city.SetBiome(biome);
                    EditorUtility.SetDirty(city);
                    biomesSet++;
                }

                if (cityViewPrefab != null)
                    SpawnOrUpdateCityView(node, city, cityViewPrefab);

                linked++;
            }

            Debug.Log($"[SPJ] City-node links updated. Linked: {linked}, Biomes set: {biomesSet}, Missing nodes: {missing}");
        }

        private static CityView FindCityViewPrefab()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab CityView",
                new[] { "Assets/Internal/Prefabs/Interactables" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null && go.TryGetComponent(out CityView _))
                    return go.GetComponent<CityView>();
            }

            Debug.LogWarning("[SPJ] CityView prefab not found in Assets/Internal/Prefabs/Interactables.");
            return null;
        }

        private static void SpawnOrUpdateCityView(Transform node, CityData city, CityView prefab)
        {
            CityView existing = node.GetComponentInChildren<CityView>();
            if (existing != null)
                Undo.DestroyObjectImmediate(existing.gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, node);
            instance.transform.localPosition = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(instance, "Create CityView");

            CityView view = instance.GetComponent<CityView>();
            view.ApplyCity(city);
        }

        private static EconomyDatabase FindEconomyDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:EconomyDatabase");
            if (guids == null || guids.Length == 0)
                return null;

            if (guids.Length > 1)
                Debug.LogWarning("[SPJ] Multiple EconomyDatabase assets found. Using the first one.");

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<EconomyDatabase>(path);
        }

        private static Dictionary<string, Biome> LoadNodeBiomesFromJson()
        {
            var result = new Dictionary<string, Biome>(StringComparer.Ordinal);
            const string jsonPath = "Assets/Internal/Models/World/Roads/_all_roads.road.json";
            if (!File.Exists(jsonPath)) return result;

            string json = File.ReadAllText(jsonPath);
            var combined = JsonUtility.FromJson<RoadJsonCombined>(json);
            if (combined?.Nodes == null) return result;

            foreach (NodeJson node in combined.Nodes)
            {
                if (string.IsNullOrWhiteSpace(node.NodeId)) continue;

                string biomeStr = (node.Biome ?? "").Trim().Replace("_", "");
                if (Enum.TryParse(biomeStr, true, out Biome b))
                    result[node.NodeId] = b;
                else
                    result[node.NodeId] = Biome.Unknown;
            }

            return result;
        }

    }
}
