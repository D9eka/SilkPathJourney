using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Internal.Scripts.Economy.Editor
{
    public static class EconomyDataImporter
    {
        private const string CSV_FOLDER = "Assets/Internal/Data/__source_csv";
        private const string GENERATED_DATA_FOLDER = "Assets/Internal/Data/Generated";
        private const string ITEMS_FOLDER = GENERATED_DATA_FOLDER + "/Items";
        private const string CITY_TYPES_FOLDER = GENERATED_DATA_FOLDER + "/CityTypes";
        private const string CITIES_FOLDER = GENERATED_DATA_FOLDER + "/Cities";
        private const string DATABASES_FOLDER = GENERATED_DATA_FOLDER + "/Databases";

        private const string LOCALIZATION_PREFIX = "loc_";
        private const string LOCALIZATION_FOLDER = GENERATED_DATA_FOLDER + "/Localization";
        private const string LOCALIZATION_LOCALES_FOLDER = LOCALIZATION_FOLDER + "/Locales";
        private const string LOCALIZATION_TABLES_FOLDER = LOCALIZATION_FOLDER + "/StringTables";
        private const string LOCALIZATION_TABLE_NAME = "Economy";

        public static void ImportAll()
        {
            try
            {
                EnsureAssetFolder(ITEMS_FOLDER);
                EnsureAssetFolder(CITY_TYPES_FOLDER);
                EnsureAssetFolder(CITIES_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);

                Dictionary<string, ItemType> itemTypeById = 
                    BuildEnumMap<ItemType>("item_categories.csv", "category_id", "enum_name");
                Dictionary<string, CityType> cityTypeById = 
                    BuildEnumMap<CityType>("city_types.csv", "city_type_id", "enum_name");
                Dictionary<string, CultureId> cultureById = 
                    BuildEnumMap<CultureId>("cultures.csv", "culture_id", "enum_name");
                HashSet<string> itemIds = BuildItemIdSet();

                Dictionary<string, LocalizationEntry> localizationEntries = CollectLocalizationEntries();
                ImportLocalization(localizationEntries);

                Dictionary<string, List<CityTypeData.CategoryCoef>> cityTypeCoefs = BuildCityTypeCoefs(itemTypeById);
                Dictionary<string, List<CityTypeData.CategoryStockProfile>> cityTypeStockProfiles =
                    BuildCityTypeStockProfiles(itemTypeById);
                List<EconomyDatabase.CultureCategoryDemandMultiplier> cultureCategoryMultipliers =
                    BuildCultureCategoryDemandMultipliers(cultureById, itemTypeById);
                List<EconomyDatabase.CultureItemDemandMultiplier> cultureItemMultipliers =
                    BuildCultureItemDemandMultipliers(cultureById, itemIds);

                List<CityTypeData> cityTypes = ImportCityTypes(cityTypeById, cityTypeCoefs, cityTypeStockProfiles);
                List<ItemData> items = ImportItems(itemTypeById);
                List<CityData> cities = ImportCities(cityTypeById, cultureById);

                UpdateDatabase(items, cityTypes, cities, cultureCategoryMultipliers, cultureItemMultipliers);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Economy data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static Dictionary<string, TEnum> BuildEnumMap<TEnum>(string csvFile, string idColumn, string enumNameColumn)
            where TEnum : struct, Enum
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, csvFile);
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            Dictionary<string, TEnum> map = new(StringComparer.Ordinal);

            if (rows.Count == 0)
                return map;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, idColumn);
            int enumIndex = FindColumnIndex(header, enumNameColumn);
            if (idIndex < 0 || enumIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing required columns in {csvFile}");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string rawName = GetField(rows[i], enumIndex).Trim();
                string enumName = string.IsNullOrWhiteSpace(rawName) ? ToPascalCase(id) : rawName;

                if (!Enum.TryParse(enumName, out TEnum value))
                {
                    Debug.LogError($"[SPJ] Enum value '{enumName}' not found for {typeof(TEnum).Name} (row {i + 1})");
                    continue;
                }

                map[id] = value;
            }

            return map;
        }

        private static Dictionary<string, List<CityTypeData.CategoryCoef>> BuildCityTypeCoefs(Dictionary<string, ItemType> itemTypeById)
        {
            Dictionary<string, List<CityTypeData.CategoryCoef>> result = 
                new Dictionary<string, List<CityTypeData.CategoryCoef>>(StringComparer.Ordinal);
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "city_type_category_coefs.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int cityTypeIndex = FindColumnIndex(header, "city_type_id");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int buyIndex = FindColumnIndex(header, "buy_coef");
            int sellIndex = FindColumnIndex(header, "sell_coef");
            if (cityTypeIndex < 0 || categoryIndex < 0 || buyIndex < 0 || sellIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_type_category_coefs.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string cityTypeId = GetField(rows[i], cityTypeIndex).Trim();
                if (string.IsNullOrWhiteSpace(cityTypeId))
                    continue;

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                if (!itemTypeById.TryGetValue(categoryId, out ItemType category))
                {
                    Debug.LogWarning($"[SPJ] Unknown category_id '{categoryId}' in city_type_category_coefs.csv (row {i + 1})");
                    category = ItemType.Unknown;
                }

                if (!TryParseFloat(GetField(rows[i], buyIndex), out float buy))
                {
                    Debug.LogWarning($"[SPJ] Invalid buy_coef '{GetField(rows[i], buyIndex)}' (row {i + 1})");
                }

                if (!TryParseFloat(GetField(rows[i], sellIndex), out float sell))
                {
                    Debug.LogWarning($"[SPJ] Invalid sell_coef '{GetField(rows[i], sellIndex)}' (row {i + 1})");
                }

                if (!result.TryGetValue(cityTypeId, out List<CityTypeData.CategoryCoef> list))
                {
                    list = new List<CityTypeData.CategoryCoef>();
                    result[cityTypeId] = list;
                }

                list.Add(new CityTypeData.CategoryCoef
                {
                    Category = category,
                    BuyCoef = buy,
                    SellCoef = sell
                });
            }

            return result;
        }

        private static Dictionary<string, List<CityTypeData.CategoryStockProfile>> BuildCityTypeStockProfiles(
            Dictionary<string, ItemType> itemTypeById)
        {
            Dictionary<string, List<CityTypeData.CategoryStockProfile>> result =
                new Dictionary<string, List<CityTypeData.CategoryStockProfile>>(StringComparer.Ordinal);

            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "city_type_category_stock_profile.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] city_type_category_stock_profile.csv not found. Stock profiles will be empty.");
                return result;
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] city_type_category_stock_profile.csv has no data rows. Defaults will be used.");
                return result;
            }

            string[] header = rows[0];
            int cityTypeIndex = FindColumnIndex(header, "city_type_id");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int desiredIndex = FindColumnIndex(header, "desired_per_scale");
            int dailyNetIndex = FindColumnIndex(header, "daily_net");
            int equilibriumIndex = FindColumnIndex(header, "equilibrium_pull");
            if (cityTypeIndex < 0 || categoryIndex < 0 || desiredIndex < 0 || dailyNetIndex < 0 || equilibriumIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_type_category_stock_profile.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string cityTypeId = GetField(rows[i], cityTypeIndex).Trim();
                if (string.IsNullOrWhiteSpace(cityTypeId))
                    continue;

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                if (!itemTypeById.TryGetValue(categoryId, out ItemType category))
                {
                    Debug.LogWarning($"[SPJ] Unknown category_id '{categoryId}' in city_type_category_stock_profile.csv (row {i + 1})");
                    category = ItemType.Unknown;
                }

                TryParseFloat(GetField(rows[i], desiredIndex), out float desiredPerScale);
                TryParseFloat(GetField(rows[i], dailyNetIndex), out float dailyNet);
                TryParseFloat(GetField(rows[i], equilibriumIndex), out float equilibriumPull);

                if (!result.TryGetValue(cityTypeId, out List<CityTypeData.CategoryStockProfile> list))
                {
                    list = new List<CityTypeData.CategoryStockProfile>();
                    result[cityTypeId] = list;
                }

                list.Add(new CityTypeData.CategoryStockProfile
                {
                    Category = category,
                    DesiredPerScale = desiredPerScale,
                    DailyNet = dailyNet,
                    EquilibriumPull = equilibriumPull
                });
            }

            return result;
        }

        private static HashSet<string> BuildItemIdSet()
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "items.csv");
            if (!File.Exists(csvPath))
                return ids;

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0)
                return ids;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "item_id");
            if (idIndex < 0)
                return ids;

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                ids.Add(id);
            }

            return ids;
        }

        private static List<EconomyDatabase.CultureCategoryDemandMultiplier> BuildCultureCategoryDemandMultipliers(
            Dictionary<string, CultureId> cultureById,
            Dictionary<string, ItemType> itemTypeById)
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "culture_category_demand_mult.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv not found. Using default multipliers.");
                return BuildDefaultCultureCategoryMultipliers(cultureById, itemTypeById);
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv has no data rows. Using default multipliers.");
                return BuildDefaultCultureCategoryMultipliers(cultureById, itemTypeById);
            }

            string[] header = rows[0];
            int cultureIndex = FindColumnIndex(header, "culture_id");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int multIndex = FindColumnIndex(header, "demand_mult");
            if (cultureIndex < 0 || categoryIndex < 0 || multIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in culture_category_demand_mult.csv");
                return new List<EconomyDatabase.CultureCategoryDemandMultiplier>();
            }

            List<EconomyDatabase.CultureCategoryDemandMultiplier> result = new();
            bool hadRow = false;
            for (int i = 1; i < rows.Count; i++)
            {
                string cultureId = GetField(rows[i], cultureIndex).Trim();
                if (string.IsNullOrWhiteSpace(cultureId))
                    continue;

                hadRow = true;
                if (!cultureById.TryGetValue(cultureId, out CultureId culture))
                {
                    Debug.LogWarning($"[SPJ] Unknown culture_id '{cultureId}' in culture_category_demand_mult.csv (row {i + 1})");
                    continue;
                }

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                if (!itemTypeById.TryGetValue(categoryId, out ItemType category))
                {
                    Debug.LogWarning($"[SPJ] Unknown category_id '{categoryId}' in culture_category_demand_mult.csv (row {i + 1})");
                    continue;
                }

                if (!TryParseFloat(GetField(rows[i], multIndex), out float mult))
                {
                    Debug.LogWarning($"[SPJ] Invalid mult '{GetField(rows[i], multIndex)}' in culture_category_demand_mult.csv (row {i + 1})");
                    continue;
                }

                result.Add(new EconomyDatabase.CultureCategoryDemandMultiplier
                {
                    Culture = culture,
                    Category = category,
                    Multiplier = mult
                });
            }

            if (!hadRow || result.Count == 0)
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv had no valid rows. Using default multipliers.");
                return BuildDefaultCultureCategoryMultipliers(cultureById, itemTypeById);
            }

            return result;
        }

        private static List<EconomyDatabase.CultureItemDemandMultiplier> BuildCultureItemDemandMultipliers(
            Dictionary<string, CultureId> cultureById,
            HashSet<string> itemIds)
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "culture_item_demand_mult.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] culture_item_demand_mult.csv not found. Using default multipliers.");
                return BuildDefaultCultureItemMultipliers(cultureById, itemIds);
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] culture_item_demand_mult.csv has no data rows. Using default multipliers.");
                return BuildDefaultCultureItemMultipliers(cultureById, itemIds);
            }

            string[] header = rows[0];
            int cultureIndex = FindColumnIndex(header, "culture_id");
            int itemIndex = FindColumnIndex(header, "item_id");
            int multIndex = FindColumnIndex(header, "demand_mult");
            if (cultureIndex < 0 || itemIndex < 0 || multIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in culture_item_demand_mult.csv");
                return new List<EconomyDatabase.CultureItemDemandMultiplier>();
            }

            List<EconomyDatabase.CultureItemDemandMultiplier> result = new();
            bool hadRow = false;
            for (int i = 1; i < rows.Count; i++)
            {
                string cultureId = GetField(rows[i], cultureIndex).Trim();
                if (string.IsNullOrWhiteSpace(cultureId))
                    continue;

                hadRow = true;
                if (!cultureById.TryGetValue(cultureId, out CultureId culture))
                {
                    Debug.LogWarning($"[SPJ] Unknown culture_id '{cultureId}' in culture_item_demand_mult.csv (row {i + 1})");
                    continue;
                }

                string itemId = GetField(rows[i], itemIndex).Trim();
                if (string.IsNullOrWhiteSpace(itemId))
                    continue;

                if (itemIds.Count > 0 && !itemIds.Contains(itemId))
                {
                    Debug.LogWarning($"[SPJ] Unknown item_id '{itemId}' in culture_item_demand_mult.csv (row {i + 1})");
                    continue;
                }

                if (!TryParseFloat(GetField(rows[i], multIndex), out float mult))
                {
                    Debug.LogWarning($"[SPJ] Invalid mult '{GetField(rows[i], multIndex)}' in culture_item_demand_mult.csv (row {i + 1})");
                    continue;
                }

                result.Add(new EconomyDatabase.CultureItemDemandMultiplier
                {
                    Culture = culture,
                    ItemId = itemId,
                    Multiplier = mult
                });
            }

            if (!hadRow || result.Count == 0)
            {
                Debug.LogWarning("[SPJ] culture_item_demand_mult.csv had no valid rows. Using default multipliers.");
                return BuildDefaultCultureItemMultipliers(cultureById, itemIds);
            }

            return result;
        }

        private static List<EconomyDatabase.CultureCategoryDemandMultiplier> BuildDefaultCultureCategoryMultipliers(
            Dictionary<string, CultureId> cultureById,
            Dictionary<string, ItemType> itemTypeById)
        {
            List<EconomyDatabase.CultureCategoryDemandMultiplier> result = new();
            List<CultureId> cultures = GetSortedCultureList(cultureById);
            List<ItemType> categories = GetSortedCategoryList(itemTypeById);

            foreach (CultureId culture in cultures)
            {
                foreach (ItemType category in categories)
                {
                    result.Add(new EconomyDatabase.CultureCategoryDemandMultiplier
                    {
                        Culture = culture,
                        Category = category,
                        Multiplier = 1f
                    });
                }
            }

            return result;
        }

        private static List<EconomyDatabase.CultureItemDemandMultiplier> BuildDefaultCultureItemMultipliers(
            Dictionary<string, CultureId> cultureById,
            HashSet<string> itemIds)
        {
            List<EconomyDatabase.CultureItemDemandMultiplier> result = new();
            List<CultureId> cultures = GetSortedCultureList(cultureById);
            List<string> items = new List<string>(itemIds);
            items.Sort(StringComparer.Ordinal);

            foreach (CultureId culture in cultures)
            {
                foreach (string itemId in items)
                {
                    result.Add(new EconomyDatabase.CultureItemDemandMultiplier
                    {
                        Culture = culture,
                        ItemId = itemId,
                        Multiplier = 1f
                    });
                }
            }

            return result;
        }

        private static List<CultureId> GetSortedCultureList(Dictionary<string, CultureId> cultureById)
        {
            List<string> keys = new List<string>(cultureById.Keys);
            keys.Sort(StringComparer.Ordinal);

            List<CultureId> cultures = new List<CultureId>();
            foreach (string key in keys)
            {
                CultureId culture = cultureById[key];
                if (culture == CultureId.None)
                    continue;
                if (!cultures.Contains(culture))
                    cultures.Add(culture);
            }

            return cultures;
        }

        private static List<ItemType> GetSortedCategoryList(Dictionary<string, ItemType> itemTypeById)
        {
            List<string> keys = new List<string>(itemTypeById.Keys);
            keys.Sort(StringComparer.Ordinal);

            List<ItemType> categories = new List<ItemType>();
            foreach (string key in keys)
            {
                ItemType category = itemTypeById[key];
                if (category == ItemType.Unknown)
                    continue;
                if (!categories.Contains(category))
                    categories.Add(category);
            }

            return categories;
        }

        private static List<ItemData> ImportItems(Dictionary<string, ItemType> itemTypeById)
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "items.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<ItemData> items = new();

            if (rows.Count == 0)
                return items;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "item_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int weightIndex = FindColumnIndex(header, "weight_kg");
            int priceIndex = FindColumnIndex(header, "base_price");
            int demandWeightIndex = FindColumnIndex(header, "demand_weight");
            if (idIndex < 0 || nameIndex < 0 || categoryIndex < 0 || weightIndex < 0 || priceIndex < 0 || demandWeightIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in items.csv (expected demand_weight)");
                return items;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                if (!itemTypeById.TryGetValue(categoryId, out ItemType type))
                {
                    Debug.LogWarning($"[SPJ] Unknown category_id '{categoryId}' in items.csv (row {i + 1})");
                    type = ItemType.Unknown;
                }

                TryParseFloat(GetField(rows[i], weightIndex), out float weight);
                TryParseInt(GetField(rows[i], priceIndex), out int price);
                TryParseFloat(GetField(rows[i], demandWeightIndex), out float demandWeight);

                ItemData asset = LoadOrCreateAsset<ItemData>(ITEMS_FOLDER, id);

                asset.ApplyImport(
                    id,
                    type,
                    weight,
                    price,
                    demandWeight,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim()));

                EditorUtility.SetDirty(asset);
                items.Add(asset);
            }

            return items;
        }

        private static List<CityTypeData> ImportCityTypes(
            Dictionary<string, CityType> cityTypeById,
            Dictionary<string, List<CityTypeData.CategoryCoef>> cityTypeCoefs,
            Dictionary<string, List<CityTypeData.CategoryStockProfile>> cityTypeStockProfiles)
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "city_types.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<CityTypeData> cityTypes = new();

            if (rows.Count == 0)
                return cityTypes;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "city_type_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            if (idIndex < 0 || nameIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_types.csv");
                return cityTypes;
            }

            bool anyProfilesLoaded = cityTypeStockProfiles.Count > 0;
            int missingProfilesCount = 0;
            int missingCoefsCount = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!cityTypeById.TryGetValue(id, out CityType type))
                {
                    Debug.LogWarning($"[SPJ] Unknown city_type_id '{id}' in city_types.csv (row {i + 1})");
                    type = CityType.Unknown;
                }

                CityTypeData asset = LoadOrCreateAsset<CityTypeData>(CITY_TYPES_FOLDER, id);

                List<CityTypeData.CategoryCoef> coefs = cityTypeCoefs.TryGetValue(id, out List<CityTypeData.CategoryCoef> coefList)
                    ? new List<CityTypeData.CategoryCoef>(coefList)
                    : new List<CityTypeData.CategoryCoef>();
                if (coefs.Count == 0)
                    missingCoefsCount++;

                List<CityTypeData.CategoryStockProfile> profiles =
                    cityTypeStockProfiles.TryGetValue(id, out List<CityTypeData.CategoryStockProfile> profileList)
                        ? new List<CityTypeData.CategoryStockProfile>(profileList)
                        : new List<CityTypeData.CategoryStockProfile>();
                if (profiles.Count == 0)
                    missingProfilesCount++;

                if (coefs.Count > 0)
                {
                    HashSet<ItemType> existing = new HashSet<ItemType>();
                    foreach (CityTypeData.CategoryStockProfile profile in profiles)
                        existing.Add(profile.Category);

                    foreach (CityTypeData.CategoryCoef coef in coefs)
                    {
                        if (existing.Contains(coef.Category))
                            continue;

                        profiles.Add(new CityTypeData.CategoryStockProfile
                        {
                            Category = coef.Category,
                            DesiredPerScale = 0f,
                            DailyNet = 0f,
                            EquilibriumPull = 0f
                        });
                    }
                }

                asset.ApplyImport(
                    type,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim()),
                    coefs,
                    profiles);

                EditorUtility.SetDirty(asset);
                cityTypes.Add(asset);
            }

            if (missingCoefsCount > 0)
                Debug.LogWarning($"[SPJ] Missing category coefs for {missingCoefsCount} city types. Check city_type_category_coefs.csv.");

            if (!anyProfilesLoaded && cityTypes.Count > 0)
                Debug.LogWarning("[SPJ] No stock profiles loaded. Defaults from category coefs were applied.");
            else if (missingProfilesCount > 0)
                Debug.LogWarning($"[SPJ] Missing stock profiles for {missingProfilesCount} city types. Defaults from category coefs were applied.");

            return cityTypes;
        }

        private static List<CityData> ImportCities(Dictionary<string, CityType> cityTypeById, Dictionary<string, CultureId> cultureById)
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, "cities.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<CityData> cities = new();

            if (rows.Count == 0)
                return cities;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "city_id");
            int nodeIndex = FindColumnIndex(header, "node_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int typeIndex = FindColumnIndex(header, "city_type_id");
            int primaryCultureIndex = FindColumnIndex(header, "primary_culture_id");
            int secondaryCultureIndex = FindColumnIndex(header, "secondary_culture_id");
            int marketScaleIndex = FindColumnIndex(header, "market_scale");
            int hasPortIndex = FindColumnIndex(header, "has_port");
            if (idIndex < 0 || nodeIndex < 0 || nameIndex < 0 || typeIndex < 0 ||
                primaryCultureIndex < 0 || secondaryCultureIndex < 0 || marketScaleIndex < 0 ||
                hasPortIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in cities.csv");
                return cities;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string typeId = GetField(rows[i], typeIndex).Trim();
                if (!cityTypeById.TryGetValue(typeId, out CityType type))
                {
                    Debug.LogWarning($"[SPJ] Unknown city_type_id '{typeId}' in cities.csv (row {i + 1})");
                    type = CityType.Unknown;
                }

                TryParseFloat(GetField(rows[i], marketScaleIndex), out float marketScale);
                bool hasPort = ParseBool(GetField(rows[i], hasPortIndex));

                CityData asset = LoadOrCreateAsset<CityData>(CITIES_FOLDER, id);

                asset.ApplyImport(
                    id,
                    GetField(rows[i], nodeIndex).Trim(),
                    type,
                    ParseCulture(GetField(rows[i], primaryCultureIndex), cultureById, i + 1, "primary_culture_id"),
                    ParseCulture(GetField(rows[i], secondaryCultureIndex), cultureById, i + 1, "secondary_culture_id"),
                    marketScale,
                    hasPort,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim()));

                EditorUtility.SetDirty(asset);
                cities.Add(asset);
            }

            return cities;
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

        private static void ImportLocalization(Dictionary<string, LocalizationEntry> entries)
        {
            HashSet<string> localeCodes = CollectLocaleCodes(entries);
            if (localeCodes.Count == 0)
            {
                Debug.LogWarning("[SPJ] No localization columns found "
                    + $"(expected columns like {LOCALIZATION_PREFIX}en, {LOCALIZATION_PREFIX}ru, ...). "
                    + $"Skipping localization import.");
                return;
            }

            List<Locale> locales = new List<Locale>();
            foreach (string code in localeCodes)
                locales.Add(EnsureLocale(code));

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LOCALIZATION_TABLE_NAME);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    LOCALIZATION_TABLE_NAME,
                    LOCALIZATION_TABLES_FOLDER,
                    locales
                );
            }

            Dictionary<string, StringTable> tablesByCode = new Dictionary<string, StringTable>(StringComparer.OrdinalIgnoreCase);
            foreach (Locale locale in locales)
            {
                StringTable table = collection.GetTable(locale.Identifier) as StringTable;
                if (table == null)
                    table = collection.AddNewTable(locale.Identifier) as StringTable;

                if (table == null)
                {
                    Debug.LogError($"[SPJ] Failed to create string table for locale '{locale.Identifier.Code}'.");
                    continue;
                }

                tablesByCode[locale.Identifier.Code] = table;
            }

            Dictionary<string, bool> tableDirty = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            bool sharedDirty = false;

            foreach (KeyValuePair<string, LocalizationEntry> kvp in entries)
            {
                string key = kvp.Key;
                LocalizationEntry entry = kvp.Value;

                foreach (string code in localeCodes)
                {
                    if (!tablesByCode.TryGetValue(code, out StringTable table))
                        continue;

                    string value = entry.TryGetValue(code, out string v) ? v : string.Empty;

                    StringTableEntry tableEntry = table.GetEntry(key);
                    if (tableEntry == null)
                    {
                        if (table.SharedData.GetEntry(key) == null)
                            sharedDirty = true;

                        tableEntry = table.AddEntry(key, value);
                        tableDirty[code] = true;
                        continue;
                    }

                    // Do not wipe existing translations if the CSV cell is empty.
                    if (!string.IsNullOrWhiteSpace(value) && tableEntry.LocalizedValue != value)
                    {
                        tableEntry.Value = value;
                        tableDirty[code] = true;
                    }
                }
            }

            foreach (KeyValuePair<string, bool> kvp in tableDirty)
            {
                if (!kvp.Value)
                    continue;
                if (tablesByCode.TryGetValue(kvp.Key, out StringTable table))
                    EditorUtility.SetDirty(table);
            }

            if (sharedDirty)
                EditorUtility.SetDirty(collection.SharedData);
        }

        private static Dictionary<string, LocalizationEntry> CollectLocalizationEntries()
        {
            Dictionary<string, LocalizationEntry> entries = new Dictionary<string, LocalizationEntry>(StringComparer.Ordinal);
            CollectLocalizationEntries(entries, "item_categories.csv");
            CollectLocalizationEntries(entries, "city_types.csv");
            CollectLocalizationEntries(entries, "items.csv");
            CollectLocalizationEntries(entries, "cities.csv");
            CollectLocalizationEntries(entries, "cultures.csv");
            return entries;
        }

        private static void CollectLocalizationEntries(Dictionary<string, LocalizationEntry> entries, string csvFile)
        {
            string csvPath = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER, csvFile);
            if (!File.Exists(csvPath))
                return;

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0)
                return;

            string[] header = rows[0];
            int keyIndex = FindColumnIndex(header, "name_key");
            Dictionary<string, int> localeColumns = FindLocaleColumns(header);
            if (keyIndex < 0 || localeColumns.Count == 0)
                return;

            for (int i = 1; i < rows.Count; i++)
            {
                string key = GetField(rows[i], keyIndex).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (!entries.TryGetValue(key, out LocalizationEntry entry))
                {
                    entry = new LocalizationEntry(key);
                    entries[key] = entry;
                }

                foreach (KeyValuePair<string, int> lc in localeColumns)
                {
                    string code = lc.Key;
                    int index = lc.Value;
                    string value = GetField(rows[i], index);
                    entry.SetIfNonEmpty(code, value, csvFile);
                }
            }
        }

        private static Locale EnsureLocale(string code)
        {
            Locale existing = LocalizationEditorSettings.GetLocale(code);
            if (existing != null)
                return existing;

            string fileSafeCode = MakeFileSafe(code);
            string assetPath = $"{LOCALIZATION_LOCALES_FOLDER}/{fileSafeCode}.asset";
            Locale locale = AssetDatabase.LoadAssetAtPath<Locale>(assetPath);
            if (locale == null)
            {
                locale = Locale.CreateLocale(code);
                locale.name = fileSafeCode;
                AssetDatabase.CreateAsset(locale, assetPath);
            }

            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static LocalizedString MakeLocalizedString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return new LocalizedString();

            return new LocalizedString(LOCALIZATION_TABLE_NAME, key);
        }

        private static int FindColumnIndex(string[] header, string columnName)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (string.Equals(header[i].Trim(), columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        private static string GetField(string[] row, int index)
        {
            if (index < 0 || index >= row.Length)
                return string.Empty;
            return row[index] ?? string.Empty;
        }

        private static bool TryParseFloat(string value, out float result)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string trimmed = value.Trim();
            if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(trimmed, "0", StringComparison.OrdinalIgnoreCase))
                return false;

            return bool.TryParse(trimmed, out bool parsed) && parsed;
        }

        private static CultureId ParseCulture(string value, Dictionary<string, CultureId> cultureById, int row, string column)
        {
            string id = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
                return CultureId.None;

            if (!cultureById.TryGetValue(id, out CultureId culture))
            {
                Debug.LogWarning($"[SPJ] Unknown {column} '{id}' in cities.csv (row {row})");
                return CultureId.None;
            }

            return culture;
        }

        private static string ToPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder sb = new System.Text.StringBuilder();
            bool nextUpper = true;
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (nextUpper)
                        sb.Append(char.ToUpperInvariant(c));
                    else
                        sb.Append(c);
                    nextUpper = false;
                }
                else
                {
                    nextUpper = true;
                }
            }
            return sb.ToString();
        }

        private static void EnsureAssetFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static T LoadOrCreateAsset<T>(string folder, string id) where T : ScriptableObject
        {
            string assetName = ToPascalCase(id);
            if (string.IsNullOrWhiteSpace(assetName))
                assetName = id;

            string newPath = $"{folder}/{assetName}.asset";
            string legacyPath = $"{folder}/{id}.asset";

            T asset = AssetDatabase.LoadAssetAtPath<T>(newPath);
            if (asset != null)
                return asset;

            T legacy = AssetDatabase.LoadAssetAtPath<T>(legacyPath);
            if (legacy != null)
            {
                string moveResult = AssetDatabase.MoveAsset(legacyPath, newPath);
                if (!string.IsNullOrEmpty(moveResult))
                {
                    Debug.LogWarning($"[SPJ] Failed to move asset '{legacyPath}' -> '{newPath}': {moveResult}");
                    return legacy;
                }

                asset = AssetDatabase.LoadAssetAtPath<T>(newPath);
                if (asset != null)
                    return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, newPath);
            return asset;
        }

        private static HashSet<string> CollectLocaleCodes(Dictionary<string, LocalizationEntry> entries)
        {
            HashSet<string> codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LocalizationEntry entry in entries.Values)
            {
                foreach (string code in entry.LocaleCodes)
                    codes.Add(code);
            }
            return codes;
        }

        private static Dictionary<string, int> FindLocaleColumns(string[] header)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Length; i++)
            {
                string raw = (header[i] ?? string.Empty).Trim();
                if (raw.Length < LOCALIZATION_PREFIX.Length)
                    continue;

                if (!raw.StartsWith(LOCALIZATION_PREFIX, StringComparison.OrdinalIgnoreCase))
                    continue;

                string code = raw.Substring(LOCALIZATION_PREFIX.Length).Trim();
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                if (!result.ContainsKey(code))
                    result[code] = i;
            }
            return result;
        }

        private static string MakeFileSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Locale";

            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '/' || c == '\\' || c == ':' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|')
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private sealed class LocalizationEntry
        {
            private readonly string _key;
            private readonly Dictionary<string, string> _valuesByLocale = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, string> _sourcesByLocale = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public IEnumerable<string> LocaleCodes => _valuesByLocale.Keys;

            public LocalizationEntry(string key)
            {
                _key = key;
            }

            public bool TryGetValue(string localeCode, out string value)
            {
                return _valuesByLocale.TryGetValue(localeCode, out value);
            }

            public void SetIfNonEmpty(string localeCode, string value, string source)
            {
                if (string.IsNullOrWhiteSpace(localeCode))
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    return;

                if (_valuesByLocale.TryGetValue(localeCode, out string existing) && existing != value)
                {
                    string prevSource = _sourcesByLocale.TryGetValue(localeCode, out string s) ? s : "?";
                    Debug.LogWarning($"[SPJ] Localization conflict for key='{_key}', locale='{localeCode}': '{existing}' ({prevSource}) -> '{value}' ({source})");
                }

                _valuesByLocale[localeCode] = value;
                _sourcesByLocale[localeCode] = source;
            }
        }
    }
}
