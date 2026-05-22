using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Internal.Scripts.Economy.Cities.Editor
{
    public sealed class CityManualSnapshotData : ScriptableObject
    {
        public enum ColliderKind { Box, Sphere, Capsule, Mesh }

        [Serializable]
        public struct ColliderEntry
        {
            public ColliderKind Kind;
            public bool Enabled;
            public bool IsTrigger;
            public Vector3 Center;
            public Vector3 BoxSize;
            public float Radius;
            public float Height;
            public int Direction;
            public string MeshGuid;
            public bool Convex;
        }

        [Serializable]
        public struct BuildingEntry
        {
            public string PrefabGuid;
            public string Name;
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
            public Vector3 LocalScale;
            public List<bool> ColliderEnabledStates;
        }

        [Serializable]
        public struct CityEntry
        {
            public string NodeId;
            public Vector3 ViewLocalPosition;
            public Quaternion ViewLocalRotation;
            public Vector3 ViewLocalScale;
            public List<ColliderEntry> ViewColliders;
            public List<BuildingEntry> Buildings;
        }

        [SerializeField] private List<CityEntry> _cities = new();

        public IReadOnlyList<CityEntry> Cities => _cities;

        public void SetData(List<CityEntry> cities)
        {
            _cities = cities;
        }
    }

    public static class CityManualSnapshotTool
    {
        private const string AssetPath = "Assets/Internal/Data/Generated/CityManualSnapshot.asset";

        [MenuItem("SPJ/City Snapshot/Save Manual City Setup")]
        public static void Save()
        {
            var views = Object.FindObjectsByType<CityView>(FindObjectsSortMode.None);
            var entries = new List<CityManualSnapshotData.CityEntry>(views.Length);
            int totalBuildings = 0;

            foreach (var view in views)
            {
                string nodeId = view.City != null ? view.City.NodeId : view.transform.parent?.name;
                if (string.IsNullOrEmpty(nodeId))
                {
                    Debug.LogWarning($"[CityManualSnapshot] CityView '{view.name}' has no NodeId and no parent name — skipping.");
                    continue;
                }

                var entry = new CityManualSnapshotData.CityEntry
                {
                    NodeId = nodeId,
                    ViewLocalPosition = view.transform.localPosition,
                    ViewLocalRotation = view.transform.localRotation,
                    ViewLocalScale = view.transform.localScale,
                    ViewColliders = SnapshotColliders(view.GetComponents<Collider>()),
                    Buildings = SnapshotBuildings(view.transform, out int count),
                };

                totalBuildings += count;
                entries.Add(entry);
            }

            var data = AssetDatabase.LoadAssetAtPath<CityManualSnapshotData>(AssetPath);
            if (data == null)
            {
                ImportHelpers.EnsureAssetFolder("Assets/Internal/Data/Generated");
                data = ScriptableObject.CreateInstance<CityManualSnapshotData>();
                AssetDatabase.CreateAsset(data, AssetPath);
            }

            data.SetData(entries);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CityManualSnapshot] Saved {entries.Count} cities, {totalBuildings} buildings → {AssetPath}");
        }

        [MenuItem("SPJ/City Snapshot/Restore Manual City Setup")]
        public static void Restore()
        {
            var data = AssetDatabase.LoadAssetAtPath<CityManualSnapshotData>(AssetPath);
            if (data == null)
            {
                Debug.LogError($"[CityManualSnapshot] No snapshot found at {AssetPath}. Run Save first.");
                return;
            }

            var nodes = ImportHelpers.BuildSceneNodeLookup("[CityManualSnapshot]");
            int restoredCities = 0;
            int restoredBuildings = 0;

            foreach (var entry in data.Cities)
            {
                if (!nodes.TryGetValue(entry.NodeId, out Transform node))
                {
                    Debug.LogWarning($"[CityManualSnapshot] Node '{entry.NodeId}' not found in scene — skipping.");
                    continue;
                }

                var view = node.GetComponentInChildren<CityView>();
                if (view == null)
                {
                    Debug.LogWarning($"[CityManualSnapshot] No CityView under node '{entry.NodeId}' — skipping.");
                    continue;
                }

                Undo.RecordObject(view.transform, "Restore CityView TRS");
                view.transform.localPosition = entry.ViewLocalPosition;
                view.transform.localRotation = entry.ViewLocalRotation;
                view.transform.localScale = entry.ViewLocalScale;

                RestoreColliders(view.gameObject, entry.ViewColliders);

                Transform existingBuildings = view.transform.Find("Buildings");
                if (existingBuildings != null)
                    Undo.DestroyObjectImmediate(existingBuildings.gameObject);

                var buildingsRoot = new GameObject("Buildings");
                Undo.RegisterCreatedObjectUndo(buildingsRoot, "Create Buildings");
                buildingsRoot.transform.SetParent(view.transform, false);
                buildingsRoot.transform.localPosition = Vector3.zero;
                buildingsRoot.transform.localRotation = Quaternion.identity;
                buildingsRoot.transform.localScale = Vector3.one;

                foreach (var building in entry.Buildings)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(building.PrefabGuid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[CityManualSnapshot] Prefab GUID '{building.PrefabGuid}' not found — skipping building '{building.Name}'.");
                        continue;
                    }

                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, buildingsRoot.transform);
                    Undo.RegisterCreatedObjectUndo(instance, "Restore Building");
                    instance.name = building.Name;

                    // TRS хранится относительно view.transform; Buildings-root не имеет смещения,
                    // поэтому localPosition/Rotation/Scale instance'а внутри buildingsRoot
                    // эквивалентны тому, что было относительно view при сохранении.
                    instance.transform.localPosition = building.LocalPosition;
                    instance.transform.localRotation = building.LocalRotation;
                    instance.transform.localScale = building.LocalScale;

                    var colliders = instance.GetComponentsInChildren<Collider>(true);
                    int applyCount = Mathf.Min(colliders.Length, building.ColliderEnabledStates?.Count ?? 0);
                    for (int i = 0; i < applyCount; i++)
                        colliders[i].enabled = building.ColliderEnabledStates[i];

                    restoredBuildings++;
                }

                restoredCities++;
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[CityManualSnapshot] Restored {restoredCities} cities, {restoredBuildings} buildings.");
        }

        private static List<CityManualSnapshotData.ColliderEntry> SnapshotColliders(Collider[] colliders)
        {
            var list = new List<CityManualSnapshotData.ColliderEntry>(colliders.Length);
            foreach (var col in colliders)
                list.Add(ReadCollider(col));
            return list;
        }

        private static CityManualSnapshotData.ColliderEntry ReadCollider(Collider col)
        {
            var e = new CityManualSnapshotData.ColliderEntry
            {
                Enabled = col.enabled,
                IsTrigger = col.isTrigger,
            };

            switch (col)
            {
                case BoxCollider box:
                    e.Kind = CityManualSnapshotData.ColliderKind.Box;
                    e.Center = box.center;
                    e.BoxSize = box.size;
                    break;
                case SphereCollider sphere:
                    e.Kind = CityManualSnapshotData.ColliderKind.Sphere;
                    e.Center = sphere.center;
                    e.Radius = sphere.radius;
                    break;
                case CapsuleCollider capsule:
                    e.Kind = CityManualSnapshotData.ColliderKind.Capsule;
                    e.Center = capsule.center;
                    e.Radius = capsule.radius;
                    e.Height = capsule.height;
                    e.Direction = capsule.direction;
                    break;
                case MeshCollider mesh:
                    e.Kind = CityManualSnapshotData.ColliderKind.Mesh;
                    e.Convex = mesh.convex;
                    if (mesh.sharedMesh != null)
                        e.MeshGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mesh.sharedMesh));
                    break;
            }

            return e;
        }

        private static void WriteCollider(Collider col, CityManualSnapshotData.ColliderEntry e)
        {
            Undo.RecordObject(col, "Restore Collider");
            col.enabled = e.Enabled;
            col.isTrigger = e.IsTrigger;

            switch (col)
            {
                case BoxCollider box:
                    box.center = e.Center;
                    box.size = e.BoxSize;
                    break;
                case SphereCollider sphere:
                    sphere.center = e.Center;
                    sphere.radius = e.Radius;
                    break;
                case CapsuleCollider capsule:
                    capsule.center = e.Center;
                    capsule.radius = e.Radius;
                    capsule.height = e.Height;
                    capsule.direction = e.Direction;
                    break;
                case MeshCollider mesh:
                    mesh.convex = e.Convex;
                    if (!string.IsNullOrEmpty(e.MeshGuid))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(e.MeshGuid);
                        mesh.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    }
                    break;
            }
        }

        private static void RestoreColliders(GameObject go, List<CityManualSnapshotData.ColliderEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            var kindCount = new Dictionary<CityManualSnapshotData.ColliderKind, int>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                kindCount.TryGetValue(e.Kind, out int prev);
                if (prev > 0)
                    Debug.LogWarning($"[CityManualSnapshot] '{go.name}' has multiple colliders of kind {e.Kind} — only the first will be restored correctly.");
                kindCount[e.Kind] = prev + 1;

                Collider col = GetOrAddCollider(go, e.Kind);
                WriteCollider(col, e);
            }
        }

        private static Collider GetOrAddCollider(GameObject go, CityManualSnapshotData.ColliderKind kind)
        {
            return kind switch
            {
                CityManualSnapshotData.ColliderKind.Box => GetOrAdd<BoxCollider>(go),
                CityManualSnapshotData.ColliderKind.Sphere => GetOrAdd<SphereCollider>(go),
                CityManualSnapshotData.ColliderKind.Capsule => GetOrAdd<CapsuleCollider>(go),
                CityManualSnapshotData.ColliderKind.Mesh => GetOrAdd<MeshCollider>(go),
                _ => GetOrAdd<BoxCollider>(go),
            };
        }

        private static T GetOrAdd<T>(GameObject go) where T : Collider
        {
            var c = go.GetComponent<T>();
            return c != null ? c : Undo.AddComponent<T>(go);
        }

        private static List<CityManualSnapshotData.BuildingEntry> SnapshotBuildings(Transform viewTransform, out int count)
        {
            count = 0;
            Transform buildingsRoot = viewTransform.Find("Buildings");
            if (buildingsRoot == null)
                return new List<CityManualSnapshotData.BuildingEntry>();

            var list = new List<CityManualSnapshotData.BuildingEntry>(buildingsRoot.childCount);

            for (int i = 0; i < buildingsRoot.childCount; i++)
            {
                var child = buildingsRoot.GetChild(i).gameObject;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(child);
                if (source == null)
                {
                    Debug.LogWarning($"[CityManualSnapshot] Building '{child.name}' has no prefab source — skipping.");
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));

                // TRS сохраняем относительно viewTransform.
                // Buildings-root не имеет смещения (localPos=0, localRot=identity, localScale=1),
                // поэтому child.localPosition/Rotation/Scale уже являются координатами
                // в пространстве view. При восстановлении мы просто присваиваем их обратно.
                var colliders = child.GetComponentsInChildren<Collider>(true);
                var enabledStates = new List<bool>(colliders.Length);
                foreach (var col in colliders)
                    enabledStates.Add(col.enabled);

                list.Add(new CityManualSnapshotData.BuildingEntry
                {
                    PrefabGuid = guid,
                    Name = child.name,
                    LocalPosition = child.transform.localPosition,
                    LocalRotation = child.transform.localRotation,
                    LocalScale = child.transform.localScale,
                    ColliderEnabledStates = enabledStates,
                });

                count++;
            }

            return list;
        }
    }
}
