using Internal.Scripts.UI.WorldLabel;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Npc.Core
{
    public class NpcView : RoadAgentView
    {
        [SerializeField] private Renderer[] _renderersToColor;
        [field: SerializeField] public string DefaultStartNodeId { get; private set; }
        [field: SerializeField] public string DefaultDestinationNodeId { get; private set; }

        private WorldCanvas _worldCanvas;
        private NpcLabelFactory _labelHelper;
        private static readonly Vector3 LabelOffset = new(0f, 2f, 0f);

        public void InitLabel(WorldCanvas worldCanvas, string npcName, LocalizedString localizedName)
        {
            _worldCanvas = worldCanvas;
            _labelHelper = new NpcLabelFactory(worldCanvas);
            NpcLabelView label = localizedName != null
                ? _labelHelper.CreateNpcLabel(
                    transform.position, LabelOffset, $"NpcLabel_{npcName}", localizedName, npcName)
                : _labelHelper.CreateNpcLabel(
                    transform.position, LabelOffset, $"NpcLabel_{npcName}", npcName);
            label?.HideIcon();
        }

        private void LateUpdate()
        {
            if (_worldCanvas != null && _labelHelper?.Label != null)
            {
                RectTransform rt = _labelHelper.Label.GetComponent<RectTransform>();
                _worldCanvas.UpdateLabelPosition(rt, transform.position, LabelOffset);
            }
        }

        private void OnDestroy()
        {
            _labelHelper?.Dispose();
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
