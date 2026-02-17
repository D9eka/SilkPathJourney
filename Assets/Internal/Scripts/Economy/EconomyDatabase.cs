using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Road.Modifiers;
using UnityEngine;

namespace Internal.Scripts.Economy
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Database", fileName = "EconomyDatabase")]
    public class EconomyDatabase : ScriptableObject
    {
        [Serializable]
        public struct CultureCategoryDemandMultiplier
        {
            [field: SerializeField] public CultureId Culture { get; set; }
            [field: SerializeField] public ItemType Category { get; set; }
            [field: SerializeField] public float Multiplier { get; set; }
        }

        [Serializable]
        public struct CultureItemDemandMultiplier
        {
            [field: SerializeField] public CultureId Culture { get; set; }
            [field: SerializeField] public string ItemId { get; set; }
            [field: SerializeField] public float Multiplier { get; set; }
        }

        [field: SerializeField] public List<ItemData> Items { get; private set; } = new();
        [field: SerializeField] public List<CityTypeData> CityTypes { get; private set; } = new();
        [field: SerializeField] public List<CityData> Cities { get; private set; } = new();
        [field: SerializeField] public List<BuildingData> Buildings { get; private set; } = new();
        [field: SerializeField] public List<RoadModifierData> RoadModifiers { get; private set; } = new();
        [field: SerializeField] public List<CityModifierData> CityModifiers { get; private set; } = new();
        [field: SerializeField] public List<CultureCategoryDemandMultiplier> CultureCategoryDemandMultipliers { get; private set; } = new();
        [field: SerializeField] public List<CultureItemDemandMultiplier> CultureItemDemandMultipliers { get; private set; } = new();

        public BuildingData GetBuilding(BuildingId buildingId)
        {
            string id = buildingId.ToString().ToLowerInvariant();
            return Buildings.Find(b => b.Id == id);
        }

        public CityTypeData GetCityType(CityType type)
        {
            return CityTypes.Find(ct => ct.Type == type);
        }

#if UNITY_EDITOR
        public void ApplyImport(
            List<ItemData> items,
            List<CityTypeData> cityTypes,
            List<CityData> cities,
            List<BuildingData> buildings,
            List<RoadModifierData> roadModifiers,
            List<CityModifierData> cityModifiers,
            List<CultureCategoryDemandMultiplier> cultureCategoryMultipliers,
            List<CultureItemDemandMultiplier> cultureItemMultipliers)
        {
            Items = items ?? new List<ItemData>();
            CityTypes = cityTypes ?? new List<CityTypeData>();
            Cities = cities ?? new List<CityData>();
            Buildings = buildings ?? new List<BuildingData>();
            RoadModifiers = roadModifiers ?? new List<RoadModifierData>();
            CityModifiers = cityModifiers ?? new List<CityModifierData>();
            CultureCategoryDemandMultipliers = cultureCategoryMultipliers ?? new List<CultureCategoryDemandMultiplier>();
            CultureItemDemandMultipliers = cultureItemMultipliers ?? new List<CultureItemDemandMultiplier>();
        }
#endif
    }
}
