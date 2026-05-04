using Internal.Scripts.Economy;
using Internal.Scripts.Events;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.RunEnd.Editor
{
    public class RunEndDebugWindow : EditorWindow
    {
        private PlayerResourceRepository _resourceRepo;
        private CrisisTrigger _crisisTrigger;

        [MenuItem("SPJ/Debug/Run End")]
        public static void ShowWindow() => GetWindow<RunEndDebugWindow>("Run End Debug");

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use", MessageType.Info);
                return;
            }

            if (!TryResolve())
            {
                EditorGUILayout.HelpBox("Scene services unavailable", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Trigger end-of-run scenario:", EditorStyles.boldLabel);
            if (GUILayout.Button("Defeat: Mutiny — Morale = 0")) TriggerMutiny();
            if (GUILayout.Button("Defeat: Bankruptcy — Money = 0")) TriggerBankruptcy();
            if (GUILayout.Button("Defeat: Famine — Food = 0")) TriggerFamine();
            if (GUILayout.Button("Defeat: Caravan Lost — Empty caravan")) TriggerCaravanLost();
        }

        private bool TryResolve()
        {
            if (_resourceRepo != null && _crisisTrigger != null)
                return true;

            SceneContext sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            var container = sceneCtx.Container;
            _resourceRepo = container.TryResolve<PlayerResourceRepository>();
            _crisisTrigger = container.TryResolve<CrisisTrigger>();

            return _resourceRepo != null && _crisisTrigger != null;
        }

        private void TriggerMutiny()
        {
            _resourceRepo.UpdateResources(s => s.Morale = 0f);
            _crisisTrigger.EditorForceCheck();
        }

        private void TriggerBankruptcy()
        {
            _resourceRepo.UpdateResources(s => s.Money = 0);
            _crisisTrigger.EditorForceCheck();
        }

        private void TriggerFamine()
        {
            _resourceRepo.UpdateResources(s => s.Food = 0f);
            _crisisTrigger.EditorForceCheck();
        }

        private void TriggerCaravanLost()
        {
            _resourceRepo.UpdateResources(s =>
            {
                s.Companions?.Clear();
                s.Carts?.Clear();
            });
            _crisisTrigger.EditorForceCheck();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                _resourceRepo = null;
                _crisisTrigger = null;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}
