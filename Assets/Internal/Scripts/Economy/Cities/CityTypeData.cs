using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
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
        [field: SerializeField] public List<CategoryCoef> CategoryCoefs { get; private set; } = new();
        [field: SerializeField] public List<CategoryStockProfile> CategoryStockProfiles { get; private set; } = new();

        protected override LocalizedString TooltipName => Name;
        protected override LocalizedString TooltipDescription => Description;
        protected override string TooltipId => Type.ToString();
        protected override string TooltipContext => "CityType";

#if UNITY_EDITOR
        public void ApplyImport(
            CityType type,
            LocalizedString name,
            LocalizedString description,
            int cityMoneyIncomePerScale,
            List<CategoryCoef> categoryCoefs,
            List<CategoryStockProfile> categoryStockProfiles)
        {
            Type = type;
            Name = name;
            Description = description ?? new LocalizedString();
            CityMoneyIncomePerScale = cityMoneyIncomePerScale;
            CategoryCoefs = categoryCoefs ?? new List<CategoryCoef>();
            CategoryStockProfiles = categoryStockProfiles ?? new List<CategoryStockProfile>();
        }
#endif
    }
}
