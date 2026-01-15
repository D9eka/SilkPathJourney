using System;
using DG.Tweening;
using Internal.Scripts.Input;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera.Zoom
{
    public class CameraZoomer : ICameraZoomer, IInitializable, IDisposable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly InputManager _inputManager;
        private readonly CameraZoomerData _cameraZoomerData;

        public float Size => _camera.fieldOfView;

        public CameraZoomer(UnityEngine.Camera camera, InputManager inputManager, CameraZoomerData cameraZoomerData)
        {
            _camera = camera;
            _inputManager = inputManager;
            _cameraZoomerData = cameraZoomerData;
        }
        
        public void Initialize()
        {
            _inputManager.OnChangeCameraSize += ChangeSize;
        }
        public void Dispose()
        {
            _inputManager.OnChangeCameraSize -= ChangeSize;
        }

        public void ZoomTo(float size, Action onComplete = null)
        {
            float duration = Mathf.Abs(Size - size) / 10f;
            ZoomTo(size, duration, onComplete);
        }

        public void ZoomTo(float size, float duration, Action onComplete = null)
        {
            _camera.DOFieldOfView(size, duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void ChangeSize(float sizeDelta)
        {
            float newSize = Size + sizeDelta * _cameraZoomerData.Sensitivity;
            newSize = Mathf.Clamp(newSize, _cameraZoomerData.MinValue, _cameraZoomerData.MaxValue);
            ZoomTo(newSize);
        }
    }
}