using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Internal.Scripts.World.Editor
{
    [InitializeOnLoad]
    public static class RoadAutoImporter
    {
        private const string SESSION_KEY = "SPJ_RoadAutoImporter_DidRun";

        static RoadAutoImporter()
        {
            EditorApplication.delayCall += RunOncePerSession;
        }

        private static void RunOncePerSession()
        {
            if (SessionState.GetBool(SESSION_KEY, false))
                return;

            SessionState.SetBool(SESSION_KEY, true);

            var guids = AssetDatabase.FindAssets("", new[] { "Assets" });
            var roadJsonPaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".road.json", StringComparison.OrdinalIgnoreCase))
                .Where(p => !p.EndsWith("_all_roads.road.json", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (roadJsonPaths.Length == 0)
            {
                Debug.Log("[SPJ] RoadAutoImporter: no .road.json found.");
                return;
            }

            Debug.Log($"[SPJ] RoadAutoImporter: reimporting {roadJsonPaths.Length} road json(s).");

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in roadJsonPaths)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log("[SPJ] RoadAutoImporter: done.");
        }
    }
}