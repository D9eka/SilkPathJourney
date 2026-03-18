using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Player.Languages.Generated;
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

        [Serializable]
        public struct CultureLanguageMapping
        {
            [field: SerializeField] public CultureId Culture { get; set; }
            [field: SerializeField] public LanguageType Language { get; set; }
        }

        [field: SerializeField] public List<ItemData> Items { get; private set; } = new();
        [field: SerializeField] public List<CityTypeData> CityTypes { get; private set; } = new();
        [field: SerializeField] public List<CityData> Cities { get; private set; } = new();
        [field: SerializeField] public List<BuildingData> Buildings { get; private set; } = new();
        [field: SerializeField] public List<RoadModifierData> RoadModifiers { get; private set; } = new();
        [field: SerializeField] public List<CityModifierData> CityModifiers { get; private set; } = new();
        [field: SerializeField] public List<CultureCategoryDemandMultiplier> CultureCategoryDemandMultipliers { get; private set; } = new();
        [field: SerializeField] public List<CultureItemDemandMultiplier> CultureItemDemandMultipliers { get; private set; } = new();
        [field: SerializeField] public List<CultureLanguageMapping> CultureLanguages { get; private set; } = new();

        public BuildingData GetBuilding(BuildingId buildingId)
        {
            var type = (BuildingType)(int)buildingId;
            return Buildings.Find(b => b.Type == type);
        }

        public List<LanguageType> GetLanguagesForCity(string cityId)
        {
            var city = Cities.Find(c => string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase));
            if (city == null) return new List<LanguageType>();

            var result = new HashSet<LanguageType>();
            foreach (var mapping in CultureLanguages)
            {
                if (mapping.Culture == city.PrimaryCulture || mapping.Culture == city.SecondaryCulture)
                    result.Add(mapping.Language);
            }
            return new List<LanguageType>(result);
        }

        public CityTypeData GetCityType(CityType type)
        {
            return CityTypes.Find(ct => ct.Type == type);
        }

        public CityModifierData GetCityModifier(string id)
        {
            return CityModifiers.Find(m => m.Id == id);
        }

        public RoadModifierData GetRoadModifier(string id)
        {
            return RoadModifiers.Find(m => m.Id == id);
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
            List<CultureItemDemandMultiplier> cultureItemMultipliers,
            List<CultureLanguageMapping> cultureLanguages)
        {
            Items = items ?? new List<ItemData>();
            CityTypes = cityTypes ?? new List<CityTypeData>();
            Cities = cities ?? new List<CityData>();
            Buildings = buildings ?? new List<BuildingData>();
            RoadModifiers = roadModifiers ?? new List<RoadModifierData>();
            CityModifiers = cityModifiers ?? new List<CityModifierData>();
            CultureCategoryDemandMultipliers = cultureCategoryMultipliers ?? new List<CultureCategoryDemandMultiplier>();
            CultureItemDemandMultipliers = cultureItemMultipliers ?? new List<CultureItemDemandMultiplier>();
            CultureLanguages = cultureLanguages ?? new List<CultureLanguageMapping>();
        }
#endif
    }
}
