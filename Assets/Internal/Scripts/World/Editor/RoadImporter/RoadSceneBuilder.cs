using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Internal.Scripts.World.Editor.RoadImporter
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

            BindCitiesToNodes();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[SPJ] Roads built. Created: {created}, Updated: {updated}, Total: {roadDatas.Length}");
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

            Dictionary<string, Transform> nodes = BuildNodeLookup();
            if (nodes.Count == 0)
            {
                Debug.LogWarning($"[SPJ] No nodes found with prefix '{NodeIdRules.NodePrefix}'. City-node links were not created.");
                return;
            }

            int linked = 0;
            int missing = 0;

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
                linked++;
            }

            Debug.Log($"[SPJ] City-node links updated. Linked: {linked}, Missing nodes: {missing}");
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

        private static Dictionary<string, Transform> BuildNodeLookup()
        {
            Dictionary<string, Transform> nodes = new Dictionary<string, Transform>(StringComparer.Ordinal);
            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

            foreach (Transform t in transforms)
            {
                if (t == null || string.IsNullOrWhiteSpace(t.name))
                    continue;

                if (!t.name.StartsWith(NodeIdRules.NodePrefix, StringComparison.Ordinal))
                    continue;

                if (nodes.ContainsKey(t.name))
                {
                    Debug.LogWarning($"[SPJ] Duplicate node id '{t.name}' found on '{t.name}'. Using the first occurrence.");
                    continue;
                }

                nodes[t.name] = t;
            }

            return nodes;
        }
    }
}
