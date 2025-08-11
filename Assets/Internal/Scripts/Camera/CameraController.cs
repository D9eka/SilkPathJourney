using System;
using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.Camera
{
    public class CameraController : MonoBehaviour
    {
        private UnityEngine.Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        public void ZoomCamera(float size, Action onComplete = null)
        {
            float duration = Math.Abs(_camera.orthographicSize - size) / 10f;
            _camera.DOOrthoSize(size, 3f)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        public void MoveCamera(Vector2 position, Action onComplete = null)
        {
            Vector3 endPosition = new Vector3(position.x, position.y, _camera.transform.position.z);
            float duration = Math.Abs(Vector3.Distance(_camera.transform.position, endPosition)) / 10f;
            _camera.transform.DOMove(endPosition, duration)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        [ContextMenu("ZoomToWorld")]
        public void ZoomToWorld()
        {
            ZoomCamera(219);
        }

        [ContextMenu("ZoomToVillage")]
        public void ZoomToVillage()
        {
            ZoomCamera(17);
        }
    }
}
