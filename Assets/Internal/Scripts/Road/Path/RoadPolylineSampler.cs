using System;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadPolylineSampler
    {
        private readonly IReadOnlyList<Vector3> _points;
        private readonly float[] _cumulativeDistance;
        
        public float Length { get; }

        public RoadPolylineSampler(IReadOnlyList<Vector3> pointsLocal)
        {
            _points = pointsLocal ?? throw new ArgumentNullException(nameof(pointsLocal));
            if (_points.Count < 2)
                throw new ArgumentException("Polyline must have at least 2 points.", nameof(pointsLocal));

            _cumulativeDistance = new float[_points.Count];
            _cumulativeDistance[0] = 0f;

            float sum = 0f;
            for (int i = 1; i < _points.Count; i++)
            {
                sum += Vector3.Distance(_points[i - 1], _points[i]);
                _cumulativeDistance[i] = sum;
            }

            Length = sum;
        }

        public Vector3 GetPositionLocal(float distanceMeters)
        {
            GetSegment(distanceMeters, out int i0, out int i1, out float t);
            return Vector3.LerpUnclamped(_points[i0], _points[i1], t);
        }

        public Vector3 GetTangentLocal(float distanceMeters)
        {
            GetSegment(distanceMeters, out int i0, out int i1, out _);
            Vector3 d = _points[i1] - _points[i0];
            return d.sqrMagnitude > 1e-8f ? d.normalized : Vector3.forward;
        }
        
        public Vector3 GetRightLocal(float distanceMeters)
        {
            Vector3 tan = GetTangentLocal(distanceMeters);
            Vector3 right = Vector3.Cross(Vector3.up, tan);
            return right.sqrMagnitude < 1e-8f ? Vector3.right : right.normalized;
        }
        
        public float ClampDistance(float distanceMeters)
        {
            return Mathf.Clamp(distanceMeters, 0f, Length);
        }

        private void GetSegment(float distanceMeters, out int i0, out int i1, out float t)
        {
            float d = ClampDistance(distanceMeters);
            if (d == 0f)
            {
                i0 = 0; i1 = 1; t = 0f;
                return;
            }
            if (Mathf.Approximately(d, Length))
            {
                i0 = _points.Count - 2; i1 = _points.Count - 1; t = 1f;
                return;
            }

            // Binary search in cumulative distances
            int lo = 0;
            int hi = _cumulativeDistance.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (_cumulativeDistance[mid] < d) lo = mid + 1;
                else hi = mid;
            }

            i1 = Mathf.Clamp(lo, 1, _points.Count - 1);
            i0 = i1 - 1;

            float segStart = _cumulativeDistance[i0];
            float segEnd = _cumulativeDistance[i1];
            float segLen = Mathf.Max(1e-6f, segEnd - segStart);

            t = (d - segStart) / segLen;
        }
    }
}
