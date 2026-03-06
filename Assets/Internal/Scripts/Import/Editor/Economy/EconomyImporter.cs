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
                var itemTypeMap = ItemCategoriesTable.Read(locEntries);
                var cityTypeMap = BuildEnumMap<CityType>("city_types.csv", "city_type_id", "enum_name");
                LocalizationImporter.CollectFromCsv(CsvPath("city_types.csv"), "name_key", locEntries);
                var cultureMap = CulturesTable.Read(locEntries);
                var itemIds = ItemsTable.ReadIds();
                LocalizationImporter.CollectFromCsv(CsvPath("items.csv"), "name_key", locEntries);
                LocalizationImporter.CollectFromCsv(CsvPath("cities.csv"), "name_key", locEntries);
                LocalizationImporter.CollectFromCsv(CsvPath("buildings.csv"), "name_key", locEntries);
                LocalizationImporter.CollectFromCsv(CsvPath("road_modifiers.csv"), "name_key", locEntries);
                LocalizationImporter.CollectFromCsv(CsvPath("city_modifiers.csv"), "name_key", locEntries);
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "city.", locEntries);
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "modifier.", locEntries);
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "city_modifier.", locEntries);

                // 4. Import localization
                LocalizationImporter.Import(locEntries, LOCALIZATION_TABLE_NAME,
                    LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                var uiEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                LocalizationImporter.CollectFromCsvWithPrefix(
                    CsvPath("localization.csv"), "key", "UI.", uiEntries);
                LocalizationImporter.CollectFromCsvWithPrefix(
                    CsvPath("interface_names.csv"), "name_key", "UI.", uiEntries);
                LocalizationImporter.Import(uiEntries, UI_LOCALIZATION_TABLE_NAME,
                    LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                // 5. Sub-tables
                var coefs = CityTypeCoefsTable.Read(itemTypeMap);
                var profiles = CityTypeStockProfileTable.Read(itemTypeMap);
                var cultureCatMult = CultureCategoryDemandTable.Read(cultureMap, itemTypeMap);
                var cultureItemMult = CultureItemDemandTable.Read(cultureMap, itemIds);

                // 6. Main tables -> assets
                var items = ItemsTable.Import(itemTypeMap, LOCALIZATION_TABLE_NAME, locEntries);
                var cityTypes = CityTypesTable.Import(cityTypeMap, coefs, profiles,
                    LOCALIZATION_TABLE_NAME, locEntries);
                var cities = CitiesTable.Import(cityTypeMap, cultureMap,
                    LOCALIZATION_TABLE_NAME, locEntries);
                var buildings = BuildingsTable.Import(LOCALIZATION_TABLE_NAME, locEntries);
                var roadModifiers = RoadModifiersTable.Import(LOCALIZATION_TABLE_NAME, locEntries);
                var cityModifiers = CityModifiersTable.Import(LOCALIZATION_TABLE_NAME, locEntries);

                // 7. Database
                UpdateDatabase(items, cityTypes, cities, buildings, roadModifiers, cityModifiers,
                    cultureCatMult, cultureItemMult);

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
            List<EconomyDatabase.CultureItemDemandMultiplier> cultureItemMultipliers)
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
                new List<EconomyDatabase.CultureCategoryDemandMultiplier>(cultureCategoryMultipliers),
                new List<EconomyDatabase.CultureItemDemandMultiplier>(cultureItemMultipliers));

            EditorUtility.SetDirty(db);
        }
    }
}
