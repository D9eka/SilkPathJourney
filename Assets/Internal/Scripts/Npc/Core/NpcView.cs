using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public class NpcView : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Renderer[] _renderersToColor;
        [field: SerializeField] public string DefaultStartNodeId { get; private set; }
        [field: SerializeField] public string DefaultDestinationNodeId { get; private set; }

        public Transform VisualRoot => _visualRoot != null ? _visualRoot : transform;

        public void SetPose(Vector3 position, Vector3 forward)
        {
            Transform t = VisualRoot;
            t.position = position;

            if (forward.sqrMagnitude > 1e-6f)
                t.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

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
