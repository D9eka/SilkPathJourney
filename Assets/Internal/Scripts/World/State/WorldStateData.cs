using UnityEngine;

namespace Internal.Scripts.World.State
{
    [System.Serializable]
    public class WorldStateData
    {
        [field: SerializeField] public WorldViewMode ViewMode { get; private set; }
        [field: SerializeField] public WorldDetailLevel DetailLevel { get; private set; }
        [field: SerializeField] public float StartCameraSize { get; private set; }
        [field: SerializeField] public float EndCameraSize { get; private set; }
    }
}
