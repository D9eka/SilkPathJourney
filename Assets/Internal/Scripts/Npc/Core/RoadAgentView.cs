using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public class RoadAgentView : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        
        public Transform VisualRoot => _visualRoot != null ? _visualRoot : transform;

        public void SetPose(Vector3 position, Vector3 forward)
        {
            Transform t = VisualRoot;
            t.position = position;

            if (forward.sqrMagnitude > 1e-6f)
            {
                Vector3 newRotation = Quaternion.LookRotation(forward, Vector3.up).eulerAngles;
                t.DORotate(newRotation, 0.3f);
            }
        }
    }
}