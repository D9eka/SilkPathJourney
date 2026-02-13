using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Buildings
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Building", fileName = "Building")]
    public class BuildingData : LocalizedTooltipData
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public BuildingType Type { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();
        [field: SerializeField] public ScreenId InteractionScreen { get; private set; } = ScreenId.None;

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Id;
        protected override string TooltipContext => "BuildingData";
        protected override string FallbackDescription => Type.ToString();

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            BuildingType type,
            LocalizedString name,
            ScreenId interactionScreen,
            LocalizedString description = null)
        {
            Id = id;
            Type = type;
            Name = name;
            InteractionScreen = interactionScreen;
            Description = description ?? new LocalizedString();
        }
#endif
    }
}
