using Internal.Scripts.Camera;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Cities
{
    [CreateAssetMenu(menuName = "SPJ/Economy/City", fileName = "City")]
    public class CityData : LocalizedTooltipData
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string NodeId { get; private set; }
        [field: SerializeField] public CityType Type { get; private set; }
        [field: SerializeField] public CultureId PrimaryCulture { get; private set; }
        [field: SerializeField] public CultureId SecondaryCulture { get; private set; }
        [field: SerializeField] public float MarketScale { get; private set; }
        [field: SerializeField] public bool HasPort { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();

        [Header("Detail Scene")]
        [Tooltip("Detail scene for this city (optional)")]
        [field: SerializeField] public SceneReference DetailScene { get; private set; }

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Id;
        protected override string TooltipContext => "CityData";
        protected override string FallbackDescription
        {
            get
            {
                string desc = $"{Type}, Market Scale: {MarketScale:F1}";
                if (HasPort) desc += ", Has Port";
                return desc;
            }
        }

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            string nodeId,
            CityType type,
            CultureId primaryCulture,
            CultureId secondaryCulture,
            float marketScale,
            bool hasPort,
            LocalizedString name,
            LocalizedString description = null)
        {
            Id = id;
            NodeId = nodeId;
            Type = type;
            PrimaryCulture = primaryCulture;
            SecondaryCulture = secondaryCulture;
            MarketScale = marketScale;
            HasPort = hasPort;
            Name = name;
            Description = description ?? new LocalizedString();
        }
#endif
    }
}
