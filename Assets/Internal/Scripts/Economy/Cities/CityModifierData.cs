using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Cities
{
    [CreateAssetMenu(menuName = "SPJ/Economy/CityModifier", fileName = "CityModifier")]
    public class CityModifierData : LocalizedTooltipData
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public float PricePct { get; private set; }
        [field: SerializeField] public float DangerPct { get; private set; }
        [field: SerializeField] public string ConflictGroup { get; private set; }
        [field: SerializeField] public string BiomeRestriction { get; private set; }
        [field: SerializeField] public int MinDuration { get; private set; }
        [field: SerializeField] public int MaxDuration { get; private set; }
        [field: SerializeField] public string CascadeRoadId { get; private set; }
        [field: SerializeField] public float CascadeChance { get; private set; }
        [field: SerializeField] public LocalizedString[] RumorLines { get; private set; } = System.Array.Empty<LocalizedString>();

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Id;
        protected override string TooltipContext => "CityModifier";
        protected override string FallbackDescription => "City effect";

        public override string GetTooltipDescription()
        {
            string desc = base.GetTooltipDescription();
            var sb = new System.Text.StringBuilder(desc);
            bool isRu = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale?.Identifier.Code?.StartsWith("ru") == true;
            if (PricePct != 0f)
                sb.Append($"\n{(isRu ? "Цены" : "Prices")}: {PricePct:+0;-0}%");
            if (DangerPct != 0f)
                sb.Append($"\n{(isRu ? "Опасность" : "Danger")}: {DangerPct:+0;-0}%");
            return sb.ToString();
        }

        public float GetSpawnWeight(Biome biome)
        {
            if (string.IsNullOrEmpty(BiomeRestriction))
                return 1f;
            return biome.ToString() == BiomeRestriction ? 1f : 0f;
        }

        public int RollDuration() => Random.Range(MinDuration, MaxDuration + 1);

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            LocalizedString name,
            LocalizedString description,
            Sprite icon,
            float pricePct,
            float dangerPct,
            string conflictGroup,
            string biomeRestriction,
            int minDuration,
            int maxDuration,
            string cascadeRoadId,
            float cascadeChance,
            LocalizedString[] rumorLines)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            PricePct = pricePct;
            DangerPct = dangerPct;
            ConflictGroup = conflictGroup;
            BiomeRestriction = biomeRestriction;
            MinDuration = minDuration;
            MaxDuration = maxDuration;
            CascadeRoadId = cascadeRoadId;
            CascadeChance = cascadeChance;
            RumorLines = rumorLines;
        }
#endif
    }
}
