using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public readonly struct IconLabelEntry
    {
        public readonly Sprite Icon;
        public readonly string Label;

        public IconLabelEntry(Sprite icon, string label)
        {
            Icon = icon;
            Label = label;
        }
    }
}
