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
        private const string UI_LOCALIZATION_TABLE_NAME = "UI";

        private static readonly string[] EconomyLocPrefixes =
        {
            "item.", "item_category.", "city.", "city_type.",
            "building.", "modifier.", "city_modifier.", "culture."
        };

        [MenuItem("SPJ/Import/Economy")]
        public static void ImportAll()
        {
            if (IsCompiling()) return;

            try
            {
                // 1. Generate enums
                ItemTypeGenerator.Generate();
                CityTypeGenerator.Generate();
                CultureIdGenerator.Generate();
                BuildingIdGenerator.Generate();

                // 2. Ensure folders
                EnsureAssetFolder(ITEMS_FOLDER);
                EnsureAssetFolder(CITY_TYPES_FOLDER);
                EnsureAssetFolder(CITIES_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);

                // 3. Read lookup tables + collect localization
                var locEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                var itemTypeMap = ItemCategoriesTable.Read();
                var cityTypeMap = BuildEnumMap<CityType>("city_types.csv", "city_type_id", "enum_name");
                var (cultureMap, cultureLanguages) = CulturesTable.Read();
                var itemIds = ItemsTable.ReadIds();
                var buildingIdMap = BuildEnumMap<BuildingId>("buildings.csv", "building_id", "enum_name");
                var cityBuildingMap = CityBuildingsTable.Read(buildingIdMap);

                string locCsv = CsvPath("localization.csv");
                foreach (string prefix in EconomyLocPrefixes)
                    LocalizationImporter.CollectFromCsvPlainLocales(locCsv, "key", prefix, locEntries);

                // 4. Import localization
                LocalizationImporter.Import(locEntries, LOCALIZATION_TABLE_NAME,
                    LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                var uiEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                LocalizationImporter.CollectFromCsvWithPrefix(
                    CsvPath("localization.csv"), "key", "UI.", uiEntries);
                LocalizationImporter.Import(uiEntries, UI_LOCALIZATION_TABLE_NAME,
                    LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                // 5. Sub-tables
                var coefs = CityTypeCoefsTable.Read(itemTypeMap);
                var profiles = CityTypeStockProfileTable.Read(itemTypeMap);
                var cultureCatMult = CultureCategoryDemandTable.Read(cultureMap, itemTypeMap);
                var cultureItemMult = CultureItemDemandTable.Read(cultureMap, itemIds);

                // 6. Main tables -> assets
                var items = ItemsTable.Import(itemTypeMap, LOCALIZATION_TABLE_NAME);
                var cityTypes = CityTypesTable.Import(cityTypeMap, coefs, profiles,
                    LOCALIZATION_TABLE_NAME);
                var cities = CitiesTable.Import(cityTypeMap, cultureMap, cityBuildingMap,
                    LOCALIZATION_TABLE_NAME);
                var buildings = BuildingsTable.Import(LOCALIZATION_TABLE_NAME);
                var roadModifiers = RoadModifiersTable.Import(LOCALIZATION_TABLE_NAME);
                var cityModifiers = CityModifiersTable.Import(LOCALIZATION_TABLE_NAME);

                // 7. Database
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
