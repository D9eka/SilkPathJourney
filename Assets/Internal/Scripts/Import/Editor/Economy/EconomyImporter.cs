using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Import.Editor.Economy.Generators;
using Internal.Scripts.Import.Editor.Economy.Tables;
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

        [MenuItem("SPJ/Import/Economy/Import All")]
        public static void ImportAll()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Cannot import while Unity is compiling. Please wait and try again.");
                return;
            }

            try
            {
                // 1. Generate enums
                ItemTypeGenerator.Generate();
                CityTypeGenerator.Generate();
                CultureIdGenerator.Generate();

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
                var items = ItemsTable.Import(itemTypeMap, LOCALIZATION_TABLE_NAME, locEntries);
                var cityTypes = CityTypesTable.Import(cityTypeMap, coefs, profiles,
                    LOCALIZATION_TABLE_NAME, locEntries);
                var cities = CitiesTable.Import(cityTypeMap, cultureMap,
                    LOCALIZATION_TABLE_NAME, locEntries);

                // 7. Database
                UpdateDatabase(items, cityTypes, cities, cultureCatMult, cultureItemMult);

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
            List<EconomyDatabase.CultureCategoryDemandMultiplier> cultureCategoryMultipliers,
            List<EconomyDatabase.CultureItemDemandMultiplier> cultureItemMultipliers)
        {
            string assetPath = $"{DATABASES_FOLDER}/EconomyDatabase.asset";
            EconomyDatabase db = AssetDatabase.LoadAssetAtPath<EconomyDatabase>(assetPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<EconomyDatabase>();
                AssetDatabase.CreateAsset(db, assetPath);
            }

            db.ApplyImport(
                items,
                cityTypes,
                cities,
                new List<EconomyDatabase.CultureCategoryDemandMultiplier>(cultureCategoryMultipliers),
                new List<EconomyDatabase.CultureItemDemandMultiplier>(cultureItemMultipliers));

            EditorUtility.SetDirty(db);
        }
    }
}
