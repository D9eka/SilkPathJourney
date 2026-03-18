using Internal.Scripts.Economy.Generated;
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
        [field: SerializeField] public float SpeedPct { get; private set; }
        [field: SerializeField] public float SuppliesPct { get; private set; }
        [field: SerializeField] public float DangerPct { get; private set; }
        [field: SerializeField] public string BiomeRestriction { get; private set; }
        [field: SerializeField] public int MinDuration { get; private set; }
        [field: SerializeField] public int MaxDuration { get; private set; }

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Id;
        protected override string TooltipContext => "RoadModifier";
        protected override string FallbackDescription => "Road effect";

        public override string GetTooltipDescription()
        {
            string desc = base.GetTooltipDescription();
            var sb = new System.Text.StringBuilder(desc);
            bool isRu = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale?.Identifier.Code?.StartsWith("ru") == true;
            if (SpeedPct != 0f)
                sb.Append($"\n{(isRu ? "Скорость" : "Speed")}: {SpeedPct:+0;-0}%");
            if (SuppliesPct != 0f)
                sb.Append($"\n{(isRu ? "Припасы" : "Supplies")}: {SuppliesPct:+0;-0}%");
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
            float speedPct,
            float suppliesPct,
            float dangerPct,
            string biomeRestriction,
            int minDuration,
            int maxDuration)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            SpeedPct = speedPct;
            SuppliesPct = suppliesPct;
            DangerPct = dangerPct;
            BiomeRestriction = biomeRestriction;
            MinDuration = minDuration;
            MaxDuration = maxDuration;
        }
#endif
    }
}
