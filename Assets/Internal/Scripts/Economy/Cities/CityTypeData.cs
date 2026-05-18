using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Cities
{
    [CreateAssetMenu(menuName = "SPJ/Economy/City Type", fileName = "CityType")]
    public class CityTypeData : LocalizedTooltipData
    {
        [Serializable]
        public struct CategoryCoef
        {
            [field: SerializeField] public ItemType Category { get; set; }
            [field: SerializeField] public float BuyCoef { get; set; }
            [field: SerializeField] public float SellCoef { get; set; }
        }

        [Serializable]
        public struct CategoryStockProfile
        {
            [field: SerializeField] public ItemType Category { get; set; }
            [field: SerializeField] public float DesiredPerScale { get; set; }
            [field: SerializeField] public float DailyNet { get; set; }
            [field: SerializeField] public float EquilibriumPull { get; set; }
        }

        [field: SerializeField] public CityType Type { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int CityMoneyIncomePerScale { get; private set; }
        [field: SerializeField] public float BaseTariffPct { get; private set; }
        [field: SerializeField] public List<CategoryCoef> CategoryCoefs { get; private set; } = new();
        [field: SerializeField] public List<CategoryStockProfile> CategoryStockProfiles { get; private set; } = new();

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Type.ToString();
        protected override string TooltipContext => "CityType";

        private const float HighThreshold = 1.05f;
        private const float LowThreshold = 0.95f;
        private const string CategoryLocTable = "Economy";

        public override string GetTooltipDescription()
        {
            string baseDesc = base.GetTooltipDescription();
            var sb = new StringBuilder(baseDesc);

            var pays = CategoryCoefs
                .Where(c => c.SellCoef > HighThreshold)
                .OrderByDescending(c => c.SellCoef)
                .Select(c => ResolveCategoryName(c.Category))
                .ToList();
            if (pays.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine().AppendLine();
                sb.Append("Купит дорого: ");
                sb.Append(string.Join(", ", pays));
            }

            var sells = CategoryCoefs
                .Where(c => c.BuyCoef < LowThreshold)
                .OrderBy(c => c.BuyCoef)
                .Select(c => ResolveCategoryName(c.Category))
                .ToList();
            if (sells.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append("Продаёт дёшево: ");
                sb.Append(string.Join(", ", sells));
            }

            return sb.ToString();
        }

        private static string ResolveCategoryName(ItemType category)
        {
            string key = $"item_category.{category.ToString().ToLowerInvariant()}.name";
            return LocalizationService.Resolve(CategoryLocTable, key, category.ToString());
        }

#if UNITY_EDITOR
        public void ApplyImport(
            CityType type,
            LocalizedString name,
            LocalizedString description,
            int cityMoneyIncomePerScale,
            float baseTariffPct,
            List<CategoryCoef> categoryCoefs,
            List<CategoryStockProfile> categoryStockProfiles)
        {
            Type = type;
            Name = name;
            Description = description ?? new LocalizedString();
            CityMoneyIncomePerScale = cityMoneyIncomePerScale;
            BaseTariffPct = baseTariffPct;
            CategoryCoefs = categoryCoefs ?? new List<CategoryCoef>();
            CategoryStockProfiles = categoryStockProfiles ?? new List<CategoryStockProfile>();
        }
#endif
    }
}
