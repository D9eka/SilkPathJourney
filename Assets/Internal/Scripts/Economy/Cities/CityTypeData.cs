using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Cities
{
    [CreateAssetMenu(menuName = "SPJ/Economy/City Type", fileName = "CityType")]
    public class CityTypeData : ScriptableObject
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
        [field: SerializeField] public int CityMoneyIncomePerScale { get; private set; }
        [field: SerializeField] public List<CategoryCoef> CategoryCoefs { get; private set; } = new();
        [field: SerializeField] public List<CategoryStockProfile> CategoryStockProfiles { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(
            CityType type,
            LocalizedString name,
            int cityMoneyIncomePerScale,
            List<CategoryCoef> categoryCoefs,
            List<CategoryStockProfile> categoryStockProfiles)
        {
            Type = type;
            Name = name;
            CityMoneyIncomePerScale = cityMoneyIncomePerScale;
            CategoryCoefs = categoryCoefs ?? new List<CategoryCoef>();
            CategoryStockProfiles = categoryStockProfiles ?? new List<CategoryStockProfile>();
        }
#endif
    }
}
