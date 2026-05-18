using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Road.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities.Editor
{
    public static class CityBuildingsBaker
    {
        private const string CatalogPath = CultureBuildingCatalog.AssetPath;

        private const float MIN_R = 0.3f;
        private const float RING_STEP = 0.3f;
        private const int   MAX_RINGS = 25;
        private const int   ATTEMPTS_PER_SLOT = 8;
        private const float ROAD_CLEARANCE = 0.05f;
        private const float BUILDING_OVERLAP_PAD = 0.02f;
        private const float SLOT_SPACING_FACTOR = 1.2f;
        private const float VORONOI_PADDING = 0.05f;
        private const string WATER_MATERIAL_NAME = "Water";

        [MenuItem("SPJ/Bake City Buildings")]
        public static void Bake()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CultureBuildingCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("[CityBuildingsBaker] CultureBuildingCatalog not found. Run SPJ/Generate Building Primitives first.");
                return;
            }

            int groundMask = GetGroundMask();
            LogGroundMaskDiagnostics(groundMask);

            var cityViews = UnityEngine.Object.FindObjectsByType<CityView>(FindObjectsSortMode.None);
            var roads = UnityEngine.Object.FindObjectsByType<RoadRuntime>(FindObjectsSortMode.None);

            var allCityXZ = new Vector2[cityViews.Length];
            for (int i = 0; i < cityViews.Length; i++)
            {
                var p = cityViews[i].transform.position;
                allCityXZ[i] = new Vector2(p.x, p.z);
            }

            int totalBuildings = 0;
            int totalGroundMisses = 0;
            var rejStats = new RejectionStats();

            for (int i = 0; i < cityViews.Length; i++)
            {
                var view = cityViews[i];
                if (view.City == null)
                {
                    Debug.LogWarning($"[CityBuildingsBaker] CityView '{view.name}' has no City assigned, skipping.");
                    continue;
                }

                var otherCityXZ = BuildOtherCitiesArray(allCityXZ, i);
                RebuildCityBuildings(view, catalog, roads, otherCityXZ, groundMask, ref totalBuildings, ref totalGroundMisses, rejStats);
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[CityBuildingsBaker] Baked {cityViews.Length} cities, total {totalBuildings} buildings. Rejections: voronoi={rejStats.Voronoi}, water={rejStats.Water}, road={rejStats.Road}, overlap={rejStats.Overlap}, groundMiss={rejStats.GroundMiss}.");

        }

        private static Vector2[] BuildOtherCitiesArray(Vector2[] all, int excludeIndex)
        {
            var arr = new Vector2[all.Length - 1];
            int j = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (i == excludeIndex) continue;
                arr[j++] = all[i];
            }
            return arr;
        }

        private class RejectionStats
        {
            public int Voronoi;
            public int Water;
            public int Road;
            public int Overlap;
            public int GroundMiss;
        }

        private static void RebuildCityBuildings(
            CityView view,
            CultureBuildingCatalog catalog,
            RoadRuntime[] allRoads,
            Vector2[] otherCityXZ,
            int groundMask,
            ref int totalBuildings,
            ref int totalGroundMisses,
            RejectionStats stats)
        {
            Transform existingBuildings = view.transform.Find("Buildings");
            if (existingBuildings != null)
                UnityEngine.Object.DestroyImmediate(existingBuildings.gameObject);

            var rawPool = catalog.GetPrefabs(view.City.PrimaryCulture);
            var pool = FilterNulls(rawPool);
            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning($"[CityBuildingsBaker] No prefabs for culture {view.City.PrimaryCulture} on city {view.City.NodeId}.");
                return;
            }

            int count = Mathf.RoundToInt(Mathf.Lerp(8f, 16f, Mathf.Clamp01(view.City.MarketScale)));

            var buildingsRoot = new GameObject("Buildings");
            buildingsRoot.transform.SetParent(view.transform, false);

            Vector3 cityCenter = view.transform.position;
            string nodeId = view.City.NodeId;

            var rng = new System.Random(nodeId.GetHashCode());
            var shuffledPool = ShufflePool(pool, rng);

            var prefabRadii = new Dictionary<GameObject, float>(shuffledPool.Length);
            var prefabBounds = new Dictionary<GameObject, Bounds>(shuffledPool.Length);
            foreach (var p in shuffledPool)
                if (!prefabRadii.ContainsKey(p))
                {
                    var (r, b) = ComputePrefabMetrics(p);
                    prefabRadii[p] = r;
                    prefabBounds[p] = b;
                }

            float cityScale = Mathf.Abs(view.transform.lossyScale.x);
            float avgPrefabRWorld = AverageRadius(shuffledPool, prefabRadii) * cityScale;
            float spacing = avgPrefabRWorld * 2f * SLOT_SPACING_FACTOR;

            var placedBounds = new List<Bounds>(count);
            int placedCount = 0;
            int prefabIdx = 0;

            for (int ringIdx = 0; ringIdx < MAX_RINGS && placedCount < count; ringIdx++)
            {
                float ringRadius = MIN_R + ringIdx * RING_STEP;
                int capacity = Mathf.Max(4, Mathf.FloorToInt(2f * Mathf.PI * ringRadius / Mathf.Max(spacing, 0.01f)));
                float angleStep = 2f * Mathf.PI / capacity;
                float ringRotation = (float)rng.NextDouble() * angleStep;

                for (int slot = 0; slot < capacity && placedCount < count; slot++)
                {
                    var prefab = shuffledPool[prefabIdx % shuffledPool.Length];
                    float prefabRadius = prefabRadii[prefab] * cityScale;
                    Bounds localBounds = prefabBounds[prefab];

                    Vector3 acceptedPos = Vector3.zero;
                    Vector3 acceptedNormal = Vector3.up;
                    Quaternion acceptedRotation = Quaternion.identity;
                    Bounds acceptedWorldBounds = default;
                    bool accepted = false;

                    for (int attempt = 0; attempt < ATTEMPTS_PER_SLOT; attempt++)
                    {
                        float jitter = ((float)rng.NextDouble() - 0.5f) * angleStep * 0.3f;
                        float angle = ringRotation + slot * angleStep + jitter;
                        float xz_x = cityCenter.x + Mathf.Cos(angle) * ringRadius;
                        float xz_z = cityCenter.z + Mathf.Sin(angle) * ringRadius;

                        if (IsCloserToOtherCity(xz_x, xz_z, cityCenter, otherCityXZ)) { stats.Voronoi++; continue; }

                        var snap = SnapToGround(xz_x, xz_z, groundMask, out float y, out Vector3 normal, ref totalGroundMisses, nodeId);
                        if (snap == SnapResult.Miss) { stats.GroundMiss++; continue; }
                        if (snap == SnapResult.Water) { stats.Water++; continue; }

                        var pos = new Vector3(xz_x, y, xz_z);

                        float margin = MinClearanceMarginToAnyRoad(pos, allRoads, prefabRadius);
                        if (margin < 0f) { stats.Road++; continue; }

                        var tilt = Quaternion.FromToRotation(Vector3.up, normal);
                        Vector3 toCenter = new Vector3(cityCenter.x - pos.x, 0f, cityCenter.z - pos.z);
                        float yawDeg = Mathf.Atan2(toCenter.x, toCenter.z) * Mathf.Rad2Deg
                                       + ((float)rng.NextDouble() - 0.5f) * 30f;
                        var rotation = Quaternion.AngleAxis(yawDeg, normal) * tilt;
                        var candidateBounds = TransformBounds(localBounds, pos, rotation, cityScale);

                        if (OverlapsPlaced(candidateBounds, placedBounds)) { stats.Overlap++; continue; }

                        acceptedPos = pos;
                        acceptedNormal = normal;
                        acceptedRotation = rotation;
                        acceptedWorldBounds = candidateBounds;
                        accepted = true;
                        break;
                    }

                    if (!accepted) continue;

                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, buildingsRoot.transform);
                    instance.transform.position = acceptedPos;
                    instance.transform.rotation = acceptedRotation;

                    placedBounds.Add(acceptedWorldBounds);
                    placedCount++;
                    prefabIdx++;
                    totalBuildings++;
                }
            }

            if (placedCount < count)
                Debug.LogWarning($"[CityBuildingsBaker] City '{nodeId}': placed only {placedCount}/{count} (no more rings fit).");
        }

        private enum SnapResult { Ok, Miss, Water }

        private static SnapResult SnapToGround(float xz_x, float xz_z, int groundMask,
            out float y, out Vector3 normal, ref int totalGroundMisses, string nodeIdForDiagnostics)
        {
            var origin = new Vector3(xz_x, 500f, xz_z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f, groundMask))
            {
                if (IsWaterHit(hit)) { y = 0f; normal = Vector3.up; return SnapResult.Water; }
                y = hit.point.y; normal = hit.normal;
                return SnapResult.Ok;
            }
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitFallback, 1000f))
            {
                if (IsWaterHit(hitFallback)) { y = 0f; normal = Vector3.up; return SnapResult.Water; }
                y = hitFallback.point.y; normal = hitFallback.normal;
                return SnapResult.Ok;
            }
            y = 0f; normal = Vector3.up;
            if (totalGroundMisses == 0)
                LogFirstMissDiagnostics(nodeIdForDiagnostics, origin, groundMask);
            totalGroundMisses++;
            return SnapResult.Miss;
        }

        private static bool IsWaterHit(RaycastHit hit)
        {
            var mc = hit.collider as MeshCollider;
            if (mc == null || mc.sharedMesh == null) return false;
            var renderer = mc.GetComponent<MeshRenderer>();
            if (renderer == null) return false;
            var mats = renderer.sharedMaterials;
            int subMeshIdx = SubMeshFromTriangle(mc.sharedMesh, hit.triangleIndex);
            if (subMeshIdx < 0 || subMeshIdx >= mats.Length) return false;
            var mat = mats[subMeshIdx];
            return mat != null && mat.name.Contains(WATER_MATERIAL_NAME);
        }

        private static int SubMeshFromTriangle(Mesh mesh, int triIndex)
        {
            int accumulated = 0;
            for (int sm = 0; sm < mesh.subMeshCount; sm++)
            {
                int tris = (int)(mesh.GetSubMesh(sm).indexCount / 3);
                if (triIndex < accumulated + tris) return sm;
                accumulated += tris;
            }
            return -1;
        }

        private static bool IsCloserToOtherCity(float x, float z, Vector3 currentCity, Vector2[] otherCitiesXZ)
        {
            if (otherCitiesXZ == null || otherCitiesXZ.Length == 0) return false;
            float dxC = x - currentCity.x;
            float dzC = z - currentCity.z;
            float distCurrent = Mathf.Sqrt(dxC * dxC + dzC * dzC);
            foreach (var other in otherCitiesXZ)
            {
                float dxO = x - other.x;
                float dzO = z - other.y;
                float distOther = Mathf.Sqrt(dxO * dxO + dzO * dzO);
                if (distOther < distCurrent + VORONOI_PADDING) return true;
            }
            return false;
        }

        private static float MinClearanceMarginToAnyRoad(Vector3 worldPos, RoadRuntime[] roads, float buildingHalfWorld)
        {
            if (roads == null || roads.Length == 0) return float.PositiveInfinity;
            float minMargin = float.PositiveInfinity;
            foreach (var road in roads)
            {
                if (road == null) continue;
                var data = road.Data;
                if (data == null || data.PointsLocal == null || data.PointsLocal.Count < 2) continue;

                float dist = float.PositiveInfinity;
                for (int k = 0; k < data.PointsLocal.Count - 1; k++)
                {
                    var aWorld = road.LocalToWorld(data.PointsLocal[k]);
                    var bWorld = road.LocalToWorld(data.PointsLocal[k + 1]);
                    float d = DistancePointToSegmentXZ(worldPos, aWorld, bWorld);
                    if (d < dist) dist = d;
                }

                float margin = dist - ROAD_CLEARANCE - buildingHalfWorld;
                if (margin < minMargin) minMargin = margin;
            }
            return minMargin;
        }

        private static float DistancePointToSegmentXZ(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector2 P = new Vector2(p.x, p.z);
            Vector2 A = new Vector2(a.x, a.z);
            Vector2 B = new Vector2(b.x, b.z);
            Vector2 AB = B - A;
            float lenSq = AB.sqrMagnitude;
            if (lenSq < 1e-8f) return Vector2.Distance(P, A);
            float t = Mathf.Clamp01(Vector2.Dot(P - A, AB) / lenSq);
            return Vector2.Distance(P, A + AB * t);
        }

        private static (float radius, Bounds localBounds) ComputePrefabMetrics(GameObject prefab)
        {
            var temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;

            var renderers = temp.GetComponentsInChildren<Renderer>();
            float r = 0.05f;
            Bounds b = new Bounds(Vector3.zero, Vector3.one * 0.1f);
            if (renderers.Length > 0)
            {
                b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                r = Mathf.Max(b.extents.x, b.extents.z);
            }

            UnityEngine.Object.DestroyImmediate(temp);
            return (Mathf.Max(r, 0.05f), b);
        }

        private static Bounds TransformBounds(Bounds local, Vector3 worldPos, Quaternion worldRot, float worldScale)
        {
            Vector3 c = worldPos + worldRot * (local.center * worldScale);
            Vector3 ex = worldRot * new Vector3(local.extents.x * worldScale, 0f, 0f);
            Vector3 ey = worldRot * new Vector3(0f, local.extents.y * worldScale, 0f);
            Vector3 ez = worldRot * new Vector3(0f, 0f, local.extents.z * worldScale);
            Vector3 abs = new Vector3(
                Mathf.Abs(ex.x) + Mathf.Abs(ey.x) + Mathf.Abs(ez.x),
                Mathf.Abs(ex.y) + Mathf.Abs(ey.y) + Mathf.Abs(ez.y),
                Mathf.Abs(ex.z) + Mathf.Abs(ey.z) + Mathf.Abs(ez.z));
            return new Bounds(c, abs * 2f);
        }

        private static float AverageRadius(GameObject[] pool, Dictionary<GameObject, float> radii)
        {
            if (pool.Length == 0) return 0.1f;
            float sum = 0f;
            foreach (var p in pool) sum += radii[p];
            return sum / pool.Length;
        }

        private static bool OverlapsPlaced(Bounds candidate, List<Bounds> placed)
        {
            Bounds expanded = candidate;
            expanded.Expand(BUILDING_OVERLAP_PAD * 2f);
            for (int i = 0; i < placed.Count; i++)
                if (expanded.Intersects(placed[i])) return true;
            return false;
        }

        private static GameObject[] FilterNulls(GameObject[] arr)
        {
            if (arr == null) return System.Array.Empty<GameObject>();
            int n = 0;
            for (int i = 0; i < arr.Length; i++) if (arr[i] != null) n++;
            var result = new GameObject[n];
            int j = 0;
            for (int i = 0; i < arr.Length; i++) if (arr[i] != null) result[j++] = arr[i];
            return result;
        }

        private static GameObject[] ShufflePool(GameObject[] pool, System.Random rng)
        {
            var arr = (GameObject[])pool.Clone();
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }

        private static void LogGroundMaskDiagnostics(int groundMask)
        {
            var layerNames = new List<string>();
            for (int i = 0; i < 32; i++)
                if ((groundMask & (1 << i)) != 0)
                    layerNames.Add($"{i}:{LayerMask.LayerToName(i)}");
            Debug.Log($"[CityBuildingsBaker] Ground mask = {groundMask}, layers = [{string.Join(", ", layerNames)}]");
        }

        private static void LogFirstMissDiagnostics(string nodeId, Vector3 origin, int groundMask)
        {
            var hitsAll = Physics.RaycastAll(origin, Vector3.down, 1000f);
            if (hitsAll.Length == 0)
            {
                Debug.LogWarning($"[CityBuildingsBaker] First miss at city '{nodeId}' origin=({origin.x:F1}, {origin.y:F1}, {origin.z:F1}): no colliders along ray (any layer). The terrain mesh likely does not extend to this XZ.");
                return;
            }

            var first = hitsAll[0];
            int hitLayer = first.collider.gameObject.layer;
            bool inMask = (groundMask & (1 << hitLayer)) != 0;
            Debug.LogWarning($"[CityBuildingsBaker] First miss at city '{nodeId}' origin=({origin.x:F1}, {origin.y:F1}, {origin.z:F1}): masked raycast missed but RaycastAll hit '{first.collider.name}' on layer {hitLayer}:{LayerMask.LayerToName(hitLayer)} (in groundMask: {inMask}). If layer is correct, ensure mask includes it; otherwise terrain is on a different layer.");
        }

        private static int GetGroundMask()
        {
            int groundOnly = LayerMask.GetMask("Ground");
            if (groundOnly != 0) return groundOnly;
            return Physics.DefaultRaycastLayers;
        }
    }
}
