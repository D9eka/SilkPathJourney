using Internal.Scripts.Road.Path;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Road.Core
{
    public sealed class RoadRuntime : MonoBehaviour
    {
        private const float THICKNESS_CENTER = 6f;
        private const float THICKNESS_LANE = 4f;
        private const float THICKNESS_EDGE = 3f;

        private static readonly Color CenterColor = new(1f, 1f, 0.2f, 1f);
        private static readonly Color EdgeColor   = new(1f, 0.6f, 0.2f, 1f);
        private static readonly Color LeftLaneColor  = new(0.2f, 1f, 1f, 1f);
        private static readonly Color RightLaneColor = new(1f, 0.4f, 1f, 1f);

        [Header("Data")]
        [SerializeField] private RoadData _data;

        [Tooltip("Root transform that 'PointsLocal' are relative to (World root in Unity).")]
        [SerializeField] private Transform _worldRoot;

        public RoadData Data => _data;
        public Transform WorldRoot => _worldRoot;

        public void SetData(RoadData data) => _data = data;
        public void SetWorldRoot(Transform root) => _worldRoot = root;

        public Vector3 LocalToWorld(Vector3 pLocal)
            => _worldRoot != null ? _worldRoot.TransformPoint(pLocal) : transform.TransformPoint(pLocal);

        public Vector3 LocalDirToWorld(Vector3 dLocal)
            => _worldRoot != null ? _worldRoot.TransformDirection(dLocal) : transform.TransformDirection(dLocal);

#if UNITY_EDITOR
        private RoadPolylineSampler _sampler;
        private float _step;
        private float _arrowEvery;
        private float _arrowSize;

        private void OnDrawGizmos()
        {
            if (_data == null || _data.PointsLocal == null || _data.PointsLocal.Count < 2)
                return;

            _sampler = new RoadPolylineSampler(_data.PointsLocal);

            float laneW = Mathf.Max(0.01f, _data.LaneWidth);
            int laneCount = Mathf.Max(1, _data.LaneCount);
            float halfRoad = laneCount * laneW * 0.5f;

            _step = Mathf.Max(1f, _data.SampleStepMeters);
            _arrowEvery = Mathf.Max(10f, _step * 5f);
            _arrowSize = Mathf.Max(0.5f, laneW * 0.6f);

            DrawEdges(halfRoad, needArrows: true);
            DrawLanes(laneCount, laneW, needArrow: true);
            DrawOffsetLine(0f, THICKNESS_CENTER, CenterColor);
        }

        private void DrawEdges(float halfRoad, bool needArrows)
        {
            DrawOffsetLine(-halfRoad, THICKNESS_EDGE, EdgeColor, needArrows);
            DrawOffsetLine(+halfRoad, THICKNESS_EDGE, EdgeColor, needArrows);
        }

        private void DrawLanes(int laneCount, float laneW, bool needArrow)
        {
            float oLaneL = (laneCount >= 2) ? -laneW * 0.5f : 0f;
            float oLaneR = (laneCount >= 2) ? +laneW * 0.5f : 0f;

            DrawOffsetLine(oLaneL, THICKNESS_LANE, LeftLaneColor, needArrow);
            DrawOffsetLine(oLaneR, THICKNESS_LANE, RightLaneColor, needArrow);
        }

        private void DrawOffsetLine(float lateralOffset, float thickness, Color color, bool drawArrows = false)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.color = color;

            Vector3 prev = SampleWorld(0f, lateralOffset);
            for (float d = _step; d <= _sampler.Length; d += _step)
            {
                Vector3 cur = SampleWorld(d, lateralOffset);
                Handles.DrawAAPolyLine(thickness, prev, cur);
                prev = cur;
            }

            Vector3 last = SampleWorld(_sampler.Length, lateralOffset);
            Handles.DrawAAPolyLine(thickness, prev, last);

            if (drawArrows)
                DrawArrows(lateralOffset);
        }

        private void DrawArrows(float lateralOffset)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.color = Color.white;

            for (float d = 0f; d <= _sampler.Length; d += _arrowEvery)
            {
                Vector3 pLocal = _sampler.GetPositionLocal(d);
                Vector3 tLocal = _sampler.GetTangentLocal(d);
                Vector3 rLocal = _sampler.GetRightLocal(d);

                Vector3 p = LocalToWorld(pLocal + rLocal * lateralOffset);
                Vector3 dir = LocalDirToWorld(tLocal).normalized;
                if (dir.sqrMagnitude < 1e-6f) continue;

                Vector3 a = p;
                Vector3 b = p + dir * _arrowSize;

                Handles.DrawAAPolyLine(3f, a, b);

                Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
                float headLen = _arrowSize * 0.35f;
                Vector3 headA = b - dir * headLen + side * headLen * 0.7f;
                Vector3 headB = b - dir * headLen - side * headLen * 0.7f;
                Handles.DrawAAPolyLine(3f, b, headA);
                Handles.DrawAAPolyLine(3f, b, headB);
            }
        }

        private Vector3 SampleWorld(float dist, float lateralOffset)
        {
            Vector3 pLocal = _sampler.GetPositionLocal(dist);
            Vector3 right = _sampler.GetRightLocal(dist);
            return LocalToWorld(pLocal + right * lateralOffset);
        }
#endif
    }
}
