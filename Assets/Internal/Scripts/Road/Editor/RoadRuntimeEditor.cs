using Internal.Scripts.Road.Core;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Road.Runtime.Editor
{
    [CustomEditor(typeof(RoadRuntime))]
    public class RoadRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var rr = (RoadRuntime)target;
            if (rr.Data == null || rr.Data.PointsLocal == null || rr.Data.PointsLocal.Count == 0)
                return;

            if (GUILayout.Button("Frame Road Start (Scene View)"))
            {
                var p = rr.Data.PointsLocal[0];
                SceneView.lastActiveSceneView?.Frame(new Bounds(p, Vector3.one * 10f), false);
            }

            if (GUILayout.Button("Frame Road End (Scene View)"))
            {
                var p = rr.Data.PointsLocal[rr.Data.PointsLocal.Count - 1];
                SceneView.lastActiveSceneView?.Frame(new Bounds(p, Vector3.one * 10f), false);
            }
        }
    }
}
