using Internal.Scripts.Camera;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Tests.Camera
{
    public class CameraTest : MonoBehaviour
    {
        [SerializeField] private Transform[] targets;
        
        [Inject] 
        private CameraController _cameraController;

        [ContextMenu("Test Focus")]
        private void TestFocus()
        {
            _cameraController.FocusOnObjects(targets);
        }
    }
}