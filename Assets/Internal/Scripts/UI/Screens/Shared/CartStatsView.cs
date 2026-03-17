using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Shared
{
    public class CartStatsView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _capacityText;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private TextMeshProUGUI _animalsText;
        [SerializeField] private TextMeshProUGUI _consumptionText;

        public void Apply(CartStatsData data)
        {
            if (_typeText != null)
                _typeText.text = data.TypeName;

            if (_speedText != null)
                _speedText.text = data.SpeedText;

            if (_capacityText != null)
                _capacityText.text = data.CapacityText;

            if (_durabilityText != null)
                _durabilityText.text = data.DurabilityText;

            if (_animalsText != null)
                _animalsText.text = data.AnimalsText;

            if (_consumptionText != null)
                _consumptionText.text = data.ConsumptionText;
        }
    }
}
