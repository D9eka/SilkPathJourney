using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    [CreateAssetMenu(menuName = "SPJ/World Canvas Settings")]
    public class WorldCanvasSettings : ScriptableObject
    {
        [Header("Canvas")]
        [field: SerializeField] public float CanvasScale { get; private set; } = 0.02f;
        [field: SerializeField] public float OffsetAboveGround { get; private set; } = 0.1f;
        [field: SerializeField] public int FontSize { get; private set; } = 36;

        [Header("Auto Scale")]
        [field: SerializeField] public float MinLabelScale { get; private set; } = 0.01f;
        [field: SerializeField] public float MaxLabelScale { get; private set; } = 0.06f;
    }
}
