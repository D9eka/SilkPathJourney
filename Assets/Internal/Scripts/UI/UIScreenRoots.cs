using UnityEngine;

namespace Internal.Scripts.UI
{
    public sealed class UIScreenRoots : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private Transform _screensRoot;
        [Header("Overlays")]
        [SerializeField] private GameObject _worldBlockerOverlay;
        [SerializeField] private GameObject _uiBlockerOverlay;

        public Transform ScreensRoot => _screensRoot;

        public void SetWorldBlockerVisible(bool visible)
        {
            if (_worldBlockerOverlay == null) return;
            _worldBlockerOverlay.SetActive(visible);
        }

        public void SetUiBlockerVisible(bool visible)
        {
            if (_uiBlockerOverlay == null) return;
            _uiBlockerOverlay.SetActive(visible);
        }
    }
}
