using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public class NpcView : RoadAgentView
    {
        [SerializeField] private Renderer[] _renderersToColor;
        [field: SerializeField] public string DefaultStartNodeId { get; private set; }
        [field: SerializeField] public string DefaultDestinationNodeId { get; private set; }

        public void ApplyColor(Color color)
        {
            if (_renderersToColor == null || _renderersToColor.Length == 0)
            {
                _renderersToColor = GetComponentsInChildren<Renderer>(includeInactive: true);
            }

            foreach (Renderer renderer in _renderersToColor)
            {
                if (renderer == null) continue;
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                        mat.color = color;
                }
            }
        }
    }
}
