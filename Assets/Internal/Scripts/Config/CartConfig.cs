using UnityEngine;

namespace Internal.Scripts.Config
{
    [CreateAssetMenu(menuName = "SPJ/Cart Config", fileName = "CartConfig")]
    public sealed class CartConfig : ScriptableObject
    {
        [field: SerializeField] public float BaseSpeed { get; private set; } = 30f;
        [field: SerializeField] public float BaseMaxDurability { get; private set; } = 100f;
        [field: SerializeField] public float BaseCapacity { get; private set; } = 250f;
        [field: SerializeField] public float FoodConsumptionPerDay { get; private set; } = 3f;
    }
}
