using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;
namespace Internal.Scripts.World.Roads
{
    [CreateAssetMenu(menuName = "SPJ/Road Data", fileName = "RoadData")]
    public class RoadData : ScriptableObject
    {
        [field:SerializeField] public int Version { get; set; }
        [field:SerializeField] public string RoadId { get; set; }

        [field:SerializeField] public string RelativeTo { get; set; }

        [Header("Meta")]
        [field: SerializeField] public CultureId Region { get; set; } = CultureId.None;
        [field: SerializeField] public int LaneCount { get; set; } = 2;
        [field:SerializeField] public float LaneWidth { get; set; } = 3.5f;
        [field:SerializeField] public float SpeedMul { get; set; } = 1f;
        [field:SerializeField] public bool Bidirectional { get; set; } = true;
        [field:SerializeField] public float SampleStepMeters { get; set; } = 2f;
        [field:SerializeField] public bool IsHidden { get; set; }

        [Header("Endpoints")]
        [field:SerializeField] public string StartNodeId { get; set; }
        [field:SerializeField] public string EndNodeId { get; set; }

        [Header("Points (LOCAL to relativeTo root)")]
        [field: SerializeField] public List<Vector3> PointsLocal { get; set; } = new();
    }
}
