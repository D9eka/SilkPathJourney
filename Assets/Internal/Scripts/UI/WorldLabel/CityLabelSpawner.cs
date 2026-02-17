using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Road.Nodes.UI;
using Internal.Scripts.Road.Nodes.UI.NodesViewer;
using Internal.Scripts.World.State;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class CityLabelSpawner : WorldLabelSpawnerBase
    {
        private const int MODIFIERS_PER_CITY = 2;

        private readonly INodesViewer _nodesViewer;
        private readonly EconomyDatabase _economyDatabase;

        public CityLabelSpawner(
            INodesViewer nodesViewer,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas,
            EconomyDatabase economyDatabase)
            : base(worldStateController, worldCanvas)
        {
            _nodesViewer = nodesViewer;
            _economyDatabase = economyDatabase;
        }

        protected override bool ShouldShowInViewMode(WorldViewMode viewMode) => true;

        protected override void SpawnLabels()
        {
            foreach (NodeView nodeView in _nodesViewer.GetAllNodes())
            {
                Transform nodeTransform = nodeView.transform.parent;
                if (nodeTransform == null) continue;
                if (!nodeTransform.TryGetComponent(out CityNodeLink link)) continue;
                if (link.City == null) continue;

                WorldLabelView label = CreateAndConfigureLabel(
                    nodeTransform.position,
                    $"CityLabel_{link.CityId}");
                label.SetLocalizedText(link.City.Name, link.City.Id);
                label.SetTooltipProvider(link.City);

                CityTypeData cityType = _economyDatabase.GetCityType(link.City.Type);
                if (cityType != null)
                {
                    if (cityType.Icon != null)
                        label.SetIcon(cityType.Icon);
                    label.SetIconTooltip(cityType.GetTooltipTitle(), cityType.GetTooltipDescription());
                }

                AddRandomModifiers(label);
            }
        }

        private void AddRandomModifiers(WorldLabelView label)
        {
            List<CityModifierData> all = _economyDatabase.CityModifiers;
            if (all == null || all.Count == 0) return;

            int count = Mathf.Min(MODIFIERS_PER_CITY, all.Count);
            List<int> indices = new(all.Count);
            for (int i = 0; i < all.Count; i++)
                indices.Add(i);

            for (int i = 0; i < count; i++)
            {
                int pick = Random.Range(i, indices.Count);
                (indices[i], indices[pick]) = (indices[pick], indices[i]);

                CityModifierData mod = all[indices[i]];
                if (mod == null) continue;

                label.AddIcon(
                    mod.Icon,
                    mod.GetTooltipTitle(),
                    mod.GetTooltipDescription());
            }
        }
    }
}
