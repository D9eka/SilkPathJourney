using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Travel.Pickups.Editor
{
    public sealed class PickupDebugWindow : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("SPJ/Debug/Pickups Log")]
        public static void Open()
        {
            GetWindow<PickupDebugWindow>("Pickups Log");
        }

        private void OnEnable()
        {
            PickupDebugLog.Changed += Repaint;
        }

        private void OnDisable()
        {
            PickupDebugLog.Changed -= Repaint;
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear", GUILayout.Width(80)))
                    PickupDebugLog.Clear();

                GUILayout.Label($"Entries: {PickupDebugLog.Entries.Count}", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(4);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (string line in PickupDebugLog.Entries)
                EditorGUILayout.LabelField(line, EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to see pickup events.", MessageType.Info);
        }
    }
}
