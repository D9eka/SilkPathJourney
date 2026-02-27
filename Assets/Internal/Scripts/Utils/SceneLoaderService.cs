using Internal.Scripts.Camera;
using Plugins.Zenject.Source.Runtime;
using UnityEngine.SceneManagement;

namespace Internal.Scripts.Utils
{
    public sealed class SceneLoaderService
    {
        private readonly TickableManager _tickableManager;

        public SceneLoaderService(TickableManager tickableManager)
        {
            _tickableManager = tickableManager;
        }

        public void LoadScene(SceneReference scene)
        {
            _tickableManager.IsPaused = true;
            SceneManager.LoadScene(scene.SceneName);
        }
    }
}
