using Internal.Scripts.Road.Path;
using Internal.Scripts.World.Roads;
using UnityEngine;

namespace Internal.Scripts.Road.Components
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
            var data = _roadRuntime.Data;
            Transform root = _roadRuntime.WorldRoot;

            Vector3 pLocal = _sampler.GetPositionLocal(dist);
            Vector3 tLocal = _sampler.GetTangentLocal(dist);
            Vector3 rightLocal = _sampler.GetRightLocal(dist);

            float laneOffset = ComputeLaneOffsetMeters(data, _lane) + _lateralOffsetMeters;
            pLocal += rightLocal * laneOffset;

            if (root != null)
            {
                transform.position = root.TransformPoint(pLocal);

                if (_alignRotationToRoad)
                {
                    Vector3 fwdWorld = root.TransformDirection(tLocal).normalized;
                    if (fwdWorld.sqrMagnitude > 1e-8f)
                        transform.rotation = Quaternion.LookRotation(fwdWorld, Vector3.up);
                }
            }
            else
            {
                // fallback (нежелательно, но пусть будет)
                transform.position = pLocal;

                if (_alignRotationToRoad)
                {
                    Vector3 fwdWorld = tLocal.normalized;
                    if (fwdWorld.sqrMagnitude > 1e-8f)
                        transform.rotation = Quaternion.LookRotation(fwdWorld, Vector3.up);
                }
            }
        }

        private static float ComputeLaneOffsetMeters(RoadData data, RoadLane lane)
        {
            if (data == null) return 0f;

            int n = Mathf.Max(1, data.LaneCount);
            float w = Mathf.Max(0.01f, data.LaneWidth);

            if (lane == RoadLane.Center || n == 1)
                return 0f;

            return lane == RoadLane.Right ? (w * 0.5f) : -(w * 0.5f);
        }
    }
}
