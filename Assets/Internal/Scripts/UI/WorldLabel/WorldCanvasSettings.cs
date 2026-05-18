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
        [field: SerializeField] public float BaseScale { get; private set; } = 0.001f;
        [field: SerializeField] public AnimationCurve DistanceScaleCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 100f, 100f);

        [Header("Prefabs")]
        [field: SerializeField] public CityLabelView LabelPrefab { get; private set; }
        [field: SerializeField] public NpcLabelView NpcLabelPrefab { get; private set; }
        [field: SerializeField] public RoadLabelView RoadLabelPrefab { get; private set; }
        [field: SerializeField] public FloatingRewardLabel FloatingRewardLabelPrefab { get; private set; }

        [Header("Always-On-Top Materials")]
        [field: SerializeField] public Material UiAlwaysOnTopMaterial { get; private set; }
        [field: SerializeField] public Material TmpAlwaysOnTopMaterial { get; private set; }
    }
}
