using Internal.Scripts.UI.Tooltip;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public readonly struct IconLabelEntry
    {
        public readonly Sprite Icon;
        public readonly string Label;
        public readonly ITooltipDataProvider TooltipProvider;

        public IconLabelEntry(Sprite icon, string label, ITooltipDataProvider tooltipProvider = null)
        {
            Icon = icon;
            Label = label;
            TooltipProvider = tooltipProvider;
        }
    }
}
