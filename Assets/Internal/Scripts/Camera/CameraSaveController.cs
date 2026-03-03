using System;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Save;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera
{
    public sealed class CameraSaveController : IInitializable, IDisposable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly ICameraZoomer _zoomer;
        private readonly CameraZoomerData _zoomerData;
        private readonly CameraSceneSettings _settings;
        private readonly CameraSceneLoader _cameraSceneLoader;
        private readonly MainSceneVisibilityController _mainSceneVisibility;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly SaveRepository _saveRepository;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerStateEvents _playerStateEvents;

        public CameraSaveController(
            UnityEngine.Camera camera,
            ICameraZoomer zoomer,
            CameraZoomerData zoomerData,
            CameraSceneSettings settings,
            CameraSceneLoader cameraSceneLoader,
            MainSceneVisibilityController mainSceneVisibility,
            IRoadNodeLookup nodeLookup,
            SaveRepository saveRepository,
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents)
        {
            _camera = camera;
            _zoomer = zoomer;
            _zoomerData = zoomerData;
            _settings = settings;
            _cameraSceneLoader = cameraSceneLoader;
            _mainSceneVisibility = mainSceneVisibility;
            _nodeLookup = nodeLookup;
            _saveRepository = saveRepository;
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
        }

        public void Initialize()
        {
            _playerStateEvents.OnCurrentNodeChanged += HandleChanged;
            _playerStateEvents.OnDestinationChanged += HandleChanged;
            Application.quitting += Save;

            CameraSaveData cameraSave = _saveRepository.Data.Camera;
            if (cameraSave != null && cameraSave.ZoomSize > 0f)
                Restore(cameraSave);
            else
                CenterOnPlayer();
        }

        public void Dispose()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleChanged;
            _playerStateEvents.OnDestinationChanged -= HandleChanged;
            Application.quitting -= Save;
        }

        private void Restore(CameraSaveData cameraSave)
        {
            float yRot = _camera.transform.eulerAngles.y;
            bool hasDetailScene = !string.IsNullOrEmpty(cameraSave.ActiveDetailScene);

            if (!hasDetailScene)
            {
                
                float targetY = _zoomerData.SizeToY(cameraSave.ZoomSize);
                _camera.transform.eulerAngles = new Vector3(_settings.StrategicTiltAngle, yRot, 0f);
                float zOffset = CameraExtensions.CalculateZOffset(targetY, _settings.StrategicTiltAngle, yRot);
                _camera.transform.position = new Vector3(
                    cameraSave.WorldTargetX, targetY, cameraSave.WorldTargetZ + zOffset);
            }
            else
            {
                float strategicY = _camera.transform.position.y;
                _camera.transform.eulerAngles = new Vector3(_settings.StrategicTiltAngle, yRot, 0f);
                float zOffset = CameraExtensions.CalculateZOffset(strategicY, _settings.StrategicTiltAngle, yRot);
                _camera.transform.position = new Vector3(
                    cameraSave.WorldTargetX, strategicY, cameraSave.WorldTargetZ + zOffset);

                float savedZoomSize = cameraSave.ZoomSize;
                _cameraSceneLoader.SuspendAutoLoading = true;
                _cameraSceneLoader.RestoreDetailScene(() =>
                {
                    _zoomer.ZoomTo(savedZoomSize, 0.5f, () =>
                    {
                        _mainSceneVisibility.Hide();
                        _cameraSceneLoader.SuspendAutoLoading = false;
                    });
                });
            }
        }

        private void CenterOnPlayer()
        {
            string currentNodeId = _saveRepository.Data.Player?.CurrentNodeId;
            if (string.IsNullOrEmpty(currentNodeId))
                return;

            Vector3? nodePos = _nodeLookup.GetPosition(currentNodeId);
            if (!nodePos.HasValue)
                return;

            float currentY = _camera.transform.position.y;
            float tiltAngle = _camera.transform.eulerAngles.x;
            float yRot = _camera.transform.eulerAngles.y;
            float zOffset = CameraExtensions.CalculateZOffset(currentY, tiltAngle, yRot);

            _camera.transform.position = new Vector3(
                nodePos.Value.x, currentY, nodePos.Value.z + zOffset);
        }

        private void HandleChanged(string _)
        {
            Save();
        }

        private void Save()
        {
            CameraSaveData save = _saveRepository.Data.Camera ?? new CameraSaveData();
            _saveRepository.Data.Camera = save;

            Vector2 worldTarget = _camera.transform.GetWorldTarget();
            save.WorldTargetX = worldTarget.x;
            save.WorldTargetZ = worldTarget.y;
            save.ZoomSize = _zoomer.Size;
            save.ActiveDetailScene = _playerStateProvider.State == PlayerState.Idle
                ? _cameraSceneLoader.ActiveDetailScene ?? string.Empty
                : string.Empty;

            _saveRepository.Save();
        }

    }
}
