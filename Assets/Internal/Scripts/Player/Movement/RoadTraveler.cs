using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player.Movement
{
    public class RoadTraveler : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private RoadRuntime _roadRuntime;

        [Header("Movement")]
        [SerializeField] private float _speedMetersPerSecond = 5f;
        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _alignRotationToRoad = true;

        [Header("Lane")]
        [SerializeField] private RoadLane _lane = RoadLane.Right;
        [Tooltip("Additional offset from lane center (meters). + = to the right.")]
        [SerializeField] private float _lateralOffsetMeters = 0f;

        [Header("State")]
        [SerializeField] private float _distanceMeters = 0f;

        private RoadPolylineSampler _sampler;
        
        [Inject] private RoadPoseSampler _poseSampler;

        private void OnEnable() => RebuildSampler();
        private void Start()
        {
            // страховка на порядок инициализации
            if (_sampler == null) RebuildSampler();
        }

        private void OnValidate()
        {
            if (_speedMetersPerSecond < 0f) _speedMetersPerSecond = 0f;
        }

        public void RebuildSampler()
        {
            if (_roadRuntime == null || _roadRuntime.Data == null ||
                _roadRuntime.Data.PointsLocal == null || _roadRuntime.Data.PointsLocal.Count < 2)
            {
                _sampler = null;
                return;
            }

            _sampler = new RoadPolylineSampler(_roadRuntime.Data.PointsLocal);
            _distanceMeters = Mathf.Clamp(_distanceMeters, 0f, _sampler.Length);
        }

        private void Update()
        {
            if (_sampler == null)
                return;

            float delta = _speedMetersPerSecond * Time.deltaTime;

            if (_loop)
            {
                _distanceMeters += delta;
                if (_sampler.Length > 1e-4f)
                    _distanceMeters = Mathf.Repeat(_distanceMeters, _sampler.Length);
            }
            else
            {
                _distanceMeters = _sampler.ClampDistance(_distanceMeters + delta);
            }

            ApplyPose(_distanceMeters);
        }

        private void ApplyPose(float dist)
        {
            RoadPose pose = _poseSampler.Sample(
                _sampler,
                dist,
                _roadRuntime.WorldRoot,
                _roadRuntime.transform,
                _roadRuntime.Data,
                _lane,
                _lateralOffsetMeters,
                isForward: true
            );

            transform.position = pose.Position;
            if (_alignRotationToRoad)
                transform.rotation = Quaternion.LookRotation(pose.Forward, Vector3.up);
        }
    }
}
