using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Path;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Arrow
{
    public class ArrowView : InteractableObjectView, IPointerClickHandler
    {
        public RoadPathSegment Segment { get; private set; }
        public GameObject RootObject { get; set; }

        [SerializeField] private Image _arrowImage;

        private UnityEngine.Camera _camera;
        private Vector3 _worldDirection;

        public void Initialize(RoadPathSegment segment, ArrowType type)
        {
            Segment = segment;
            if (_arrowImage != null)
                _arrowImage.color = GetColor(type);
        }

        public void SetDirection(Vector3 worldDirection, UnityEngine.Camera camera)
        {
            _worldDirection = worldDirection;
            _camera = camera;
        }

        private void LateUpdate()
        {
            if (_camera == null || _worldDirection.sqrMagnitude < 0.001f) return;

            float x = Vector3.Dot(_worldDirection, _camera.transform.right);
            float y = Vector3.Dot(_worldDirection, _camera.transform.up);
            float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    
        protected override void OnClickEffect()
        {
            base.OnClickEffect();
            transform.localScale *= 1.3f;
        }
    
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerClick();
        }

        private Color GetColor(ArrowType type) => type switch
        {
            ArrowType.Fastest => Color.yellow,
            ArrowType.Good => Color.cyan,
            _ => Color.gray
        };
    }
}
