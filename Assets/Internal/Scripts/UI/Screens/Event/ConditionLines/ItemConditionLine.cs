using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Localization.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Event.ConditionLines
{
    public class ItemConditionLine : IConditionLine
    {
        private readonly ItemCatalog _itemCatalog;

        public ItemConditionLine(ItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog;
        }

        public string GetLine(ConditionLineContext context)
        {
            if (context.Choice.Conditions == null || context.Choice.Conditions.Count == 0)
                return null;

            string format = null;
            List<string> lines = null;

            foreach (EventCondition cond in context.Choice.Conditions)
            {
                if (cond.Type != EventConditionType.HasItem) continue;

                format ??= LocalizationService.Resolve(LocEvents.Table, LocEvents.Event_ItemConditionInfo);

                string itemName = _itemCatalog.ResolveItemName(cond.Param);
                int count = Mathf.RoundToInt(cond.Value);

                lines ??= new List<string>();
                lines.Add(LocArgRenderer.Format(format, new List<ILocArg>
                {
                    new TextLocArg("item_name", itemName),
                    new TextLocArg("count", count.ToString())
                }));
            }

            return lines == null ? null : string.Join("\n", lines);
        }
    }
}
