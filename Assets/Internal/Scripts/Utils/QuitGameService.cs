using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Internal.Scripts.Utils
{
    public sealed class QuitGameService
    {
        public void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
