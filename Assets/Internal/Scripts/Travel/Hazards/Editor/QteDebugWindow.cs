using Internal.Scripts.Travel.Hazards;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Editor
{
    public sealed class QteDebugWindow : EditorWindow
    {
        private HazardData[] _hazards;
        private string[] _hazardNames;
        private int _selectedIndex;

        [MenuItem("SPJ/Debug/QTE Debug Window")]
        public static void Open()
        {
            GetWindow<QteDebugWindow>("QTE Debug");
        }

        private void OnEnable()
        {
            RefreshHazards();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
                RefreshHazards();

            if (_hazards == null || _hazards.Length == 0)
            {
                EditorGUILayout.HelpBox("No HazardData assets found.", MessageType.Warning);
                return;
            }

            _selectedIndex = EditorGUILayout.Popup("Hazard", _selectedIndex, _hazardNames);

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            if (GUILayout.Button("Play"))
                PlaySelected();

            if (GUILayout.Button("Play Random"))
                PlayRandom();

            EditorGUI.EndDisabledGroup();

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to test QTE.", MessageType.Info);
        }

        private void RefreshHazards()
        {
            var guids = AssetDatabase.FindAssets("t:HazardData");
            _hazards = new HazardData[guids.Length];
            _hazardNames = new string[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                _hazards[i] = AssetDatabase.LoadAssetAtPath<HazardData>(path);
                _hazardNames[i] = _hazards[i] != null ? _hazards[i].name : path;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, _hazards.Length - 1));
        }

        private void PlaySelected()
        {
            if (_hazards == null || _selectedIndex >= _hazards.Length) return;
            TriggerHazard(_hazards[_selectedIndex]);
        }

        private void PlayRandom()
        {
            if (_hazards == null || _hazards.Length == 0) return;
            TriggerHazard(_hazards[Random.Range(0, _hazards.Length)]);
        }

        private static void TriggerHazard(HazardData data)
        {
            if (data == null) return;

            if (!Application.isPlaying)
            {
                Debug.LogWarning("QteDebugWindow: Enter Play Mode first.");
                return;
            }

            var sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null)
            {
                Debug.LogWarning("QteDebugWindow: SceneContext not found.");
                return;
            }

            var controller = sceneCtx.Container.TryResolve<HazardController>();
            if (controller == null)
            {
                Debug.LogWarning("QteDebugWindow: HazardController not found in SceneContext.");
                return;
            }

            controller.ForceShow(data);
        }
    }
}
