using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Internal.Scripts.World.Editor.RoadImporter
{
    [InitializeOnLoad]
    public static class RoadAutoSceneBuilder
    {
        private const string PREF_KEY = "SPJ_Roads_AutoBuildInScene";

        static RoadAutoSceneBuilder()
        {
            if (!EditorPrefs.HasKey(PREF_KEY))
                EditorPrefs.SetBool(PREF_KEY, true);

            EditorApplication.delayCall += TryBuild;

            EditorSceneManager.sceneOpened += (_, __) => TryBuild();
        }

        [MenuItem("SPJ/Roads/Auto Build Roads In Scene/Toggle")]
        public static void Toggle()
        {
            bool v = EditorPrefs.GetBool(PREF_KEY, true);
            EditorPrefs.SetBool(PREF_KEY, !v);
            Debug.Log($"[SPJ] Auto Build Roads In Scene: {!v}");
        }

        [MenuItem("SPJ/Roads/Auto Build Roads In Scene/Toggle", true)]
        public static bool ToggleValidate()
        {
            Menu.SetChecked("SPJ/Roads/Auto Build Roads In Scene/Toggle",
                EditorPrefs.GetBool(PREF_KEY, true));
            return true;
        }

        private static void TryBuild()
        {
            if (!EditorPrefs.GetBool(PREF_KEY, true))
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            if (GameObject.Find("SPJ_Roads") != null)
                return;

            RoadSceneBuilder.BuildRoadsInCurrentScene();
        }
    }
}
