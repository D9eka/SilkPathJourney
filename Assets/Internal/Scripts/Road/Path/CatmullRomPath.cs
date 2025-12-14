using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Road.Paths
{
    /// <summary>
    /// Catmull-Rom путь на 4 контрольных точках.
    /// Параметризация по длине через таблицу arcLength(t).
    /// Выдаёт готовые точки для рендера (SampleByDistance).
    /// </summary>
    public sealed class CatmullRomPath : ICurvePath
    {
        private readonly Vector3 _p0, _p1, _p2, _p3;

        private readonly float[] _arcLengths;
        private readonly int _samples;
        private readonly float _length;

        private readonly Vector3[] _positions;

        public float Length => _length;

        public CatmullRomPath(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples = 32)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;

            _samples = Mathf.Max(4, samples);
            _arcLengths = new float[_samples + 1];
            _positions  = new Vector3[_samples + 1];

            _length = BuildArcLengthTableAndPositions();
        }

        private float BuildArcLengthTableAndPositions()
        {
            float length = 0f;

            _arcLengths[0] = 0f;
            _positions[0]  = Evaluate(0f);

            for (int i = 1; i <= _samples; i++)
            {
                float t = (float)i / _samples;
                _positions[i] = Evaluate(t);

                length += Vector3.Distance(_positions[i - 1], _positions[i]);
                _arcLengths[i] = length;
            }

            return length;
        }

        public Vector3 GetPosition(float distance)
        {
            if (_length <= 0f) return _p1;

            float d = Mathf.Clamp(distance, 0f, _length);
            float t = DistanceToT(d);
            return Evaluate(t);
        }

        public Vector3 GetTangent(float distance)
        {
            if (_length <= 0f) return Vector3.right;

            float d = Mathf.Clamp(distance, 0f, _length);
            float t = DistanceToT(d);

            const float eps = 1f / 1000f;
            float t1 = Mathf.Clamp01(t - eps);
            float t2 = Mathf.Clamp01(t + eps);

            Vector3 a = Evaluate(t1);
            Vector3 b = Evaluate(t2);

            Vector3 tangent = (b - a);
            if (tangent.sqrMagnitude < 1e-8f) return Vector3.right;

            return tangent.normalized;
        }

        public IReadOnlyList<Vector3> SampleByDistance(int segments)
        {
            int seg = Mathf.Max(1, segments);
            int count = seg + 1;

            var pts = new List<Vector3>(count);

            if (_length <= 0f)
            {
                pts.Add(_p1);
                pts.Add(_p2);
                return pts;
            }

            float step = _length / seg;
            for (int i = 0; i <= seg; i++)
            {
                float dist = step * i;
                pts.Add(GetPosition(dist));
            }

            return pts;
        }

        private float DistanceToT(float distance)
        {
            if (distance <= 0f) return 0f;
            if (distance >= _length) return 1f;

            int low = 0;
            int high = _samples;

            while (low < high)
            {
                int mid = (low + high) / 2;
                if (_arcLengths[mid] < distance)
                    low = mid + 1;
                else
                    high = mid;
            }

            int i = Mathf.Clamp(low, 1, _samples);
            float d0 = _arcLengths[i - 1];
            float d1 = _arcLengths[i];

            float t0 = (float)(i - 1) / _samples;
            float t1 = (float)i / _samples;

            float denom = (d1 - d0);
            if (denom <= 1e-6f) return t0;

            float u = (distance - d0) / denom;
            return Mathf.Lerp(t0, t1, u);
        }

        private Vector3 Evaluate(float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * _p1) +
                (-_p0 + _p2) * t +
                (2f * _p0 - 5f * _p1 + 4f * _p2 - _p3) * t2 +
                (-_p0 + 3f * _p1 - 3f * _p2 + _p3) * t3
            );
        }
    }
}
