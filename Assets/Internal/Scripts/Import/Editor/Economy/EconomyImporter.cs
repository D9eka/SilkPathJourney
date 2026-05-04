using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Import.Editor.Economy.Generators;
using Internal.Scripts.Import.Editor.Economy.Tables;
using Internal.Scripts.Road.Modifiers;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy
{
    public static class EconomyImporter
    {
        private const string ITEMS_FOLDER = GENERATED_DATA_FOLDER + "/Items";
        private const string CITY_TYPES_FOLDER = GENERATED_DATA_FOLDER + "/CityTypes";
        private const string CITIES_FOLDER = GENERATED_DATA_FOLDER + "/Cities";
        private const string LOCALIZATION_TABLE_NAME = "Economy";

        [MenuItem("SPJ/Import/Economy/Generate")]
        public static void Generate()
        {
            ItemTypeGenerator.Generate();
            CityTypeGenerator.Generate();
            CultureIdGenerator.Generate();
            BuildingIdGenerator.Generate();
        }

        [MenuItem("SPJ/Import/Economy/Import")]
        public static void Import()
        {
            if (IsCompiling()) return;

            try
            {
                // 2. Ensure folders
                EnsureAssetFolder(ITEMS_FOLDER);
                EnsureAssetFolder(CITY_TYPES_FOLDER);
                EnsureAssetFolder(CITIES_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                // 3. Read lookup tables
                var itemTypeMap = ItemCategoriesTable.Read();
                var cityTypeMap = BuildEnumMap<CityType>("city_types.csv", "city_type_id", "enum_name");
                var (cultureMap, cultureLanguages) = CulturesTable.Read();
                var itemIds = ItemsTable.ReadIds();
                var buildingIdMap = BuildEnumMap<BuildingId>("buildings.csv", "building_id", "enum_name");
                var cityBuildingMap = CityBuildingsTable.Read(buildingIdMap);

                // 4. Sub-tables
                var coefs = CityTypeCoefsTable.Read(itemTypeMap);
                var profiles = CityTypeStockProfileTable.Read(itemTypeMap);
                var cultureCatMult = CultureCategoryDemandTable.Read(cultureMap, itemTypeMap);
                var cultureItemMult = CultureItemDemandTable.Read(cultureMap, itemIds);

                // 5. Main tables -> assets
                var items = ItemsTable.Import(itemTypeMap, LOCALIZATION_TABLE_NAME);
                var cityTypes = CityTypesTable.Import(cityTypeMap, coefs, profiles,
                    LOCALIZATION_TABLE_NAME);
                var cities = CitiesTable.Import(cityTypeMap, cultureMap, cityBuildingMap,
                    LOCALIZATION_TABLE_NAME);
                var buildings = BuildingsTable.Import(LOCALIZATION_TABLE_NAME);
                var roadModifiers = RoadModifiersTable.Import(LOCALIZATION_TABLE_NAME);
                var cityModifiers = CityModifiersTable.Import(LOCALIZATION_TABLE_NAME);

                // 6. Database
                UpdateDatabase(items, cityTypes, cities, buildings, roadModifiers, cityModifiers,
                    cultureCatMult, cultureItemMult, cultureLanguages);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Economy data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void UpdateDatabase(
            List<ItemData> items,
            List<CityTypeData> cityTypes,
            List<CityData> cities,
            List<BuildingData> buildings,
            List<RoadModifierData> roadModifiers,
            List<CityModifierData> cityModifiers,
            List<EconomyDatabase.CultureCategoryDemandMultiplier> cultureCategoryMultipliers,
            List<EconomyDatabase.CultureItemDemandMultiplier> cultureItemMultipliers,
            List<EconomyDatabase.CultureLanguageMapping> cultureLanguages)
        {
            string assetPath = $"{DATABASES_FOLDER}/EconomyDatabase.asset";
            EconomyDatabase db = LoadOrCreateAsset<EconomyDatabase>(assetPath);

            db.ApplyImport(
                items,
                cityTypes,
                cities,
                buildings,
                roadModifiers,
                cityModifiers,
                cultureCategoryMultipliers,
                cultureItemMultipliers,
                cultureLanguages);

            EditorUtility.SetDirty(db);
        }
    }
}
