using UnityEngine;
using Internal.Scripts.World.State;

namespace Internal.Scripts.World.VisualObjects
{
    [DisallowMultipleComponent]
    public class MonoBehVisualObject : MonoBehaviour, IVisualObject
    {
        [field: SerializeField] public WorldDetailLevel ViewMode { get; private set; } = WorldDetailLevel.Both;

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
        
        public void EditorSetViewMode(WorldDetailLevel mode) => ViewMode = mode;
    }
}