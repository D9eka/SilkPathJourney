using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public class RoadAgentView : MonoBehaviour
    {
        public const float ROAD_GROUND_OFFSET = 0.5f;

        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _rotationSpeed = 10f;

        public Transform VisualRoot => _visualRoot != null ? _visualRoot : transform;

        public void SetPose(Vector3 position, Vector3 forward)
        {
            Transform t = VisualRoot;
            t.position = position + Vector3.up * ROAD_GROUND_OFFSET;
            SmoothRotate(t, forward, _rotationSpeed);
        }

        public static void SmoothRotate(Transform t, Vector3 forward, float rotationSpeed)
        {
            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude > 1e-6f)
            {
                Quaternion target = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
                float smoothFactor = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
                t.rotation = Quaternion.Slerp(t.rotation, target, smoothFactor);
            }
        }
    }
}