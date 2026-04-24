using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public sealed class FloatingRewardSpawner
    {
        private readonly WorldCanvas _worldCanvas;
        private readonly WorldCanvasSettings _settings;
        private readonly ResourceIconCatalog _iconCatalog;
        private readonly ItemCatalog _itemCatalog;

        public FloatingRewardSpawner(
            WorldCanvas worldCanvas,
            WorldCanvasSettings settings,
            ResourceIconCatalog iconCatalog,
            ItemCatalog itemCatalog)
        {
            _worldCanvas = worldCanvas;
            _settings = settings;
            _iconCatalog = iconCatalog;
            _itemCatalog = itemCatalog;
        }

        public void Show(Vector3 worldPosition, EventOutcomeEntry entry)
        {
            if (_settings == null || _settings.FloatingRewardLabelPrefab == null)
            {
                Debug.LogWarning("[FloatingRewardSpawner] FloatingRewardLabelPrefab is null in WorldCanvasSettings");
                return;
            }

            Sprite icon = null;
            string sign = entry.Value >= 0 ? "+" : string.Empty;
            string amount = $"{sign}{entry.Value:F0}";
            string text;

            ResourceType? resType = entry.Type.ToResourceType();
            if (resType.HasValue)
            {
                icon = _iconCatalog?.Get(resType.Value)?.Icon;
                text = amount;
            }
            else if (entry.Type == EventOutcomeType.AddItem && !string.IsNullOrEmpty(entry.Param))
            {
                string itemName = _itemCatalog.ResolveItemName(entry.Param);
                text = $"{amount} {itemName}";
            }
            else
            {
                return;
            }

            RectTransform root = _worldCanvas.CreatePositionedRoot(
                worldPosition, Vector3.zero, "FloatingReward");
            if (root == null)
                return;

            FloatingRewardLabel label = Object.Instantiate(
                _settings.FloatingRewardLabelPrefab, root);
            _worldCanvas.AddBillboard(label.gameObject);
            label.Play(icon, text);
        }
    }
}
