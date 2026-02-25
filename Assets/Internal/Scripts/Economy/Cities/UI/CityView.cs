using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities.UI
{
    public class CityView : InteractableObjectView
    {
        [field: SerializeField] public CityData City { get; private set; }

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
