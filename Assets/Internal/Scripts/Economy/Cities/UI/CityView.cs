using DG.Tweening;
using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities.UI
{
    public class CityView : InteractableObjectView
    {
        [field: SerializeField] public CityData City { get; private set; }

        public bool IsHoverEnabled { get; set; } = true;

        public override void TriggerHoverEnter()
        {
            if (!IsHoverEnabled) return;
            base.TriggerHoverEnter();
        }

        private const float MIN_Y_SCALE = 0.125f;

        public void SetYScaleFactor(float factor)
        {
            transform.DOKill();
            var s = OriginalScale;
            s.y = Mathf.Max(OriginalScale.y * Mathf.Clamp01(factor), MIN_Y_SCALE);
            transform.localScale = s;
        }

        public void AnimateRestoreScale(float duration = 0.3f)
        {
            transform.DOScale(OriginalScale, duration);
        }

        public void ApplyBiomeColor(Color color)
        {
            var meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
                meshRenderer.material.color = color;
        }

#if UNITY_EDITOR
        public void ApplyCity(CityData city)
        {
            City = city;
            float s = Mathf.Lerp(0.7f, 1.3f, city.MarketScale);
            transform.localScale = transform.localScale * s;
        }
#endif
    }
}
