using System;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Path;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Arrow
{
    public class ArrowView : InteractableObjectView, IPointerClickHandler
    {
        public RoadPathSegment Segment { get; private set; }

        private Image _arrowImage;
        private TextMeshProUGUI _nameText;
        private Sprite _arrowSprite;

        public void InjectDependencies(Image arrowImage, TextMeshProUGUI nameText, Sprite arrowSprite)
        {
            _arrowImage = arrowImage;
            _nameText = nameText;
            _arrowSprite = arrowSprite;
        }

        public void Initialize(RoadPathSegment segment, ArrowType type)
        {
            Segment = segment;
            if (_arrowImage != null)
            {
                _arrowImage.sprite = _arrowSprite;
                _arrowImage.color = GetColor(type);
            }
        }
    
        public void SetDirection(Vector3 worldDirection)
        {
            // Billboard rotation is handled by WorldCanvas
            // For UI arrows, we can use simple Z-axis rotation for direction indication
            float angle = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, -angle);
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
