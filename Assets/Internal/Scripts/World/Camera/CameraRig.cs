using UnityEngine;

namespace Internal.Scripts.World.Camera
{
    public class CameraRig : ICameraRig
    {
        private readonly UnityEngine.Camera _camera;
        private readonly float _sensitivity;

        public CameraRig(UnityEngine.Camera camera, float sensitivity)
        {
            _camera = camera;
            _sensitivity = sensitivity;
        }

        public float Size => _camera.orthographicSize;

        public void ChangeSize(float sizeDelta)
        {
            _camera.orthographicSize += sizeDelta * _sensitivity;
        }
    }
}