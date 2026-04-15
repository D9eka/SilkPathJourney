using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    [CreateAssetMenu(menuName = "SPJ/World Canvas Settings")]
    public class WorldCanvasSettings : ScriptableObject
    {
        [Header("Canvas")]
        [field: SerializeField] public float CanvasScale { get; private set; } = 0.02f;
        [field: SerializeField] public float OffsetAboveGround { get; private set; } = 0.1f;

        [Header("Auto Scale")]
        [field: SerializeField] public float MinLabelScale { get; private set; } = 0.01f;
        [field: SerializeField] public float MaxLabelScale { get; private set; } = 0.06f;

        [Header("Prefabs")]
        [field: SerializeField] public CityLabelView LabelPrefab { get; private set; }
        [field: SerializeField] public NpcLabelView NpcLabelPrefab { get; private set; }
        [field: SerializeField] public RoadLabelView RoadLabelPrefab { get; private set; }
        [field: SerializeField] public FloatingRewardLabel FloatingRewardLabelPrefab { get; private set; }
    }
}
