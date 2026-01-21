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
            Vector3 safeDir = SafeNormalize(worldDirection);
            transform.rotation = Quaternion.LookRotation(safeDir, Vector3.up);
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
    }
}
