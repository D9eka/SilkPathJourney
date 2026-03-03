using UnityEngine;

namespace Internal.Scripts.Camera
{
    public static class CameraExtensions
    {
        public static Vector2 GetWorldTarget(this Transform camTransform)
        {
            Vector3 pos = camTransform.position;
            Vector3 forward = camTransform.forward;
            if (Mathf.Abs(forward.y) < 0.001f)
                return new Vector2(pos.x, pos.z);
            float zOffset = pos.y * forward.z / forward.y;
            return new Vector2(pos.x, pos.z - zOffset);
        }

        public static float GetZOffset(this Transform camTransform)
        {
            Vector3 forward = camTransform.forward;
            if (Mathf.Abs(forward.y) < 0.001f) return 0f;
            return camTransform.position.y * forward.z / forward.y;
        }

        public static float CalculateZOffset(float cameraY, float xAngleDeg, float yAngleDeg)
        {
            float xRad = xAngleDeg * Mathf.Deg2Rad;
            float yRad = yAngleDeg * Mathf.Deg2Rad;
            float forwardY = -Mathf.Sin(xRad);
            if (Mathf.Abs(forwardY) < 0.001f) return 0f;
            float forwardZ = Mathf.Cos(xRad) * Mathf.Cos(yRad);
            return cameraY * forwardZ / forwardY;
        }
    }
}
