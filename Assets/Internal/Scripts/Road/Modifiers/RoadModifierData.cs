using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Road.Modifiers
{
    [CreateAssetMenu(menuName = "SPJ/Road/Modifier", fileName = "RoadModifier")]
    public class RoadModifierData : LocalizedTooltipData
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();
        [field: SerializeField] public Sprite Icon { get; private set; }

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Id;
        protected override string TooltipContext => "RoadModifier";
        protected override string FallbackDescription => "Road effect";

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            LocalizedString name,
            LocalizedString description,
            Sprite icon)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
        }
#endif
    }
}
