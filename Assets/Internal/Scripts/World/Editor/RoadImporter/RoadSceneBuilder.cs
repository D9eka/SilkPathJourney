using System.Linq;
using Internal.Scripts.Road.Core;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Internal.Scripts.World.Editor
{
    public static class RoadSceneBuilder
    {
        private const string RoadsRootName = "SPJ_Roads";

        [MenuItem("SilkPathJourney/Roads/Build Roads In Current Scene")]
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
    }
}
