using System;
using System.IO;
using Internal.Scripts.World;
using Internal.Scripts.World.Editor.DTO;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.World.Editor
{
    public class RoadJsonPostprocessor : AssetPostprocessor
    {
        private const string ALL_ROADS_FILE_NAME = "_all_roads.road.json";
        private const string ROAD_ASSETS_FOLDER = "Assets/Internal/World/Roads";

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (string path in imported)
            {
                if (!path.EndsWith(ALL_ROADS_FILE_NAME, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    EnsureFolderExists(ROAD_ASSETS_FOLDER);

                    string json = File.ReadAllText(path);
                    RoadJsonCombined combined = JsonUtility.FromJson<RoadJsonCombined>(json);

                    if (combined?.Roads == null || combined.Roads.Length == 0)
                    {
                        Debug.LogWarning($"[SPJ] '{ALL_ROADS_FILE_NAME}' has no roads: {path}");
                        continue;
                    }

                    foreach (RoadJsonSingle r in combined.Roads)
                        Upsert(ROAD_ASSETS_FOLDER, r, path);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"[SPJ] Imported roads: {combined.Roads.Length} from {path}");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static void Upsert(string assetDir, RoadJsonSingle dto, string srcPath)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoadId))
            {
                Debug.LogError($"[SPJ] Invalid road json entry in {srcPath}");
                return;
            }

            if (dto.PointsLocal == null || dto.PointsLocal.Length < 2)
            {
                Debug.LogError($"[SPJ] Invalid pointsLocal in {srcPath} (roadId={dto.RoadId})");
                return;
            }

            string assetPath = $"{assetDir}/{dto.RoadId}.asset".Replace('\\', '/');

            RoadData roadData = AssetDatabase.LoadAssetAtPath<RoadData>(assetPath);
            if (roadData == null)
            {
                roadData = ScriptableObject.CreateInstance<RoadData>();
                AssetDatabase.CreateAsset(roadData, assetPath);
            }

            roadData.Version = dto.Version;
            roadData.RoadId = dto.RoadId;
            roadData.RelativeTo = dto.RelativeTo ?? "";

            roadData.LaneCount = dto.Meta?.LaneCount ?? 2;
            roadData.LaneWidth = dto.Meta?.LaneWidth ?? 3.5f;
            roadData.SpeedMul = dto.Meta?.SpeedMul ?? 1f;
            roadData.Bidirectional = dto.Meta?.Bidirectional ?? true;
            roadData.SampleStepMeters = dto.Meta?.SampleStepMeters ?? 2f;

            roadData.StartNodeId = dto.Endpoints?.StartNodeId ?? "";
            roadData.EndNodeId = dto.Endpoints?.EndNodeId ?? "";

            roadData.PointsLocal.Clear();
            foreach (Point p in dto.PointsLocal)
                roadData.PointsLocal.Add(new Vector3(p.X, p.Y, p.Z));

            EditorUtility.SetDirty(roadData);
        }

        private static void EnsureFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string[] parts = folder.Split('/');
            string current = parts[0]; 
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
