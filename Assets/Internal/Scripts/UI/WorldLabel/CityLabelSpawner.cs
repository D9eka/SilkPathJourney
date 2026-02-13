using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Road.Nodes.UI;
using Internal.Scripts.Road.Nodes.UI.NodesViewer;
using Internal.Scripts.World.State;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class CityLabelSpawner : WorldLabelSpawnerBase
    {
        private readonly INodesViewer _nodesViewer;

        public CityLabelSpawner(
            INodesViewer nodesViewer,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas)
            : base(worldStateController, worldCanvas)
        {
            _nodesViewer = nodesViewer;
        }

        protected override bool ShouldShowInViewMode(WorldViewMode viewMode) => true;

        protected override void SpawnLabels()
        {
            foreach (NodeView nodeView in _nodesViewer.GetAllNodes())
            {
                Transform nodeTransform = nodeView.transform.parent;
                if (nodeTransform == null) continue;
                if (!nodeTransform.TryGetComponent<CityNodeLink>(out CityNodeLink link)) continue;
                if (link.City == null) continue;

                WorldLabelView label = CreateAndConfigureLabel(
                    nodeTransform.position,
                    $"CityLabel_{link.CityId}");
                label.SetLocalizedText(link.City.Name, link.City.Id);
                label.SetTooltipProvider(link.City);
            }
        }
    }
}
