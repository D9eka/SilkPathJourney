using System;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Path;
using UnityEngine;
namespace Internal.Scripts.Player.UI.Arrow
{
    public class ArrowView : InteractableObjectView
    {
        public RoadPathSegment Segment { get; private set; }
    
        [SerializeField] private MeshRenderer _arrowRenderer;
    
        public void Initialize(RoadPathSegment segment, ArrowType type)
        {
            Segment = segment;
            _arrowRenderer.material.color = GetColor(type);
        }
    
        public void SetDirection(Vector3 worldDirection)
        {
            Vector3 up = Vector3.up;
            if (TryGetGroundNormal(out Vector3 groundNormal))
            {
                up = groundNormal;
            }

            Vector3 forward = Vector3.ProjectOnPlane(worldDirection, up);
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.ProjectOnPlane(transform.forward, up);
            }
            Vector3 safeDir = SafeNormalize(forward);
            transform.rotation = Quaternion.LookRotation(safeDir, up);
        }
    
        protected override void OnClickEffect()
        {
            base.OnClickEffect();
            transform.localScale *= 1.3f;
        }
    
        private Color GetColor(ArrowType type) => type switch
        {
            ArrowType.Fastest => Color.yellow,
            ArrowType.Good => Color.cyan,
            _ => Color.gray
        };
    
        private static Vector3 SafeNormalize(Vector3 dir)
        {
            return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward;
        }

        private bool TryGetGroundNormal(out Vector3 normal)
        {
            Vector3 origin = transform.position + Vector3.up * 50f;
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 200f, ~0, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
            {
                normal = Vector3.up;
                return false;
            }

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == null)
                    continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                normal = hit.normal;
                return true;
            }

            normal = Vector3.up;
            return false;
        }
    }
}
