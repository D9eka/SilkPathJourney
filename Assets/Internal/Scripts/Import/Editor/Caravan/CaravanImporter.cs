using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Caravan.Generators;
using Internal.Scripts.Import.Editor.Caravan.Tables;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan
{
    public static class CaravanImporter
    {
        private const string CARAVAN_FOLDER = GENERATED_DATA_FOLDER + "/Caravan";

        [MenuItem("SPJ/Import/Caravan")]
        public static void ImportAll()
        {
            if (IsCompiling()) return;

            try
            {
                CartClassGenerator.Generate();
                CartUpgradeLevelGenerator.Generate();
                ExtraCartTypeGenerator.Generate();
                CompanionTypeGenerator.Generate();
                CompanionQualityGenerator.Generate();
                CaravanUpgradeTypeGenerator.Generate();
                DraftAnimalTypeGenerator.Generate();

                EnsureAssetFolder(CARAVAN_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                var classMap = BuildEnumMap<CartClass>("cart_classes.csv", "class_id", "enum_name");
                var upgradeLevelMap = BuildEnumMap<CartUpgradeLevel>("cart_upgrade_levels.csv", "level_id", "enum_name");
                var extraCartMap = BuildEnumMap<ExtraCartType>("extra_carts.csv", "extra_cart_id", "enum_name");
                var companionTypeMap = BuildEnumMap<CompanionType>("companion_types.csv", "type_id", "enum_name");
                var companionQualityMap = BuildEnumMap<CompanionQuality>("companion_quality_levels.csv", "quality_id", "enum_name");
                var upgradeTypeMap = BuildEnumMap<CaravanUpgradeType>("caravan_upgrades.csv", "upgrade_id", "enum_name");
                var animalTypeMap = BuildEnumMap<DraftAnimalType>("draft_animals.csv", "animal_id", "enum_name");
                var buildingMap = BuildEnumMap<BuildingId>("buildings.csv", "building_id", "enum_name");
                var biomeMap = BuildEnumMap<Biome>("biome_palettes.csv", "biome_id", "enum_name");

                var upgradeLevels = CartUpgradeLevelsTable.Read(upgradeLevelMap);
                var companionQualities = CompanionQualityLevelsTable.Read(companionQualityMap);
                var companionBonuses = CompanionBonusesTable.Read(companionTypeMap, companionQualityMap);
                var caravanUpgrades = CaravanUpgradesTable.Read(upgradeTypeMap, buildingMap);
                var animalBiome = AnimalBiomeTable.Read(animalTypeMap, biomeMap);

                var cartClasses = CartClassesTable.Import(classMap);
                var extraCarts = ExtraCartsTable.Import(extraCartMap);
                var companionTypes = CompanionTypesTable.Import(companionTypeMap);
                var draftAnimals = DraftAnimalsTable.Import(animalTypeMap);

                UpdateDatabase(cartClasses, extraCarts, companionTypes, draftAnimals,
                    upgradeLevels, companionQualities, companionBonuses, caravanUpgrades, animalBiome);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Caravan data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void UpdateDatabase(
            List<CartClassData> cartClasses,
            List<ExtraCartData> extraCarts,
            List<CompanionTypeData> companionTypes,
            List<DraftAnimalData> draftAnimals,
            List<CaravanDatabase.CartUpgradeLevelEntry> upgradeLevels,
            List<CaravanDatabase.CompanionQualityEntry> companionQualities,
            List<CaravanDatabase.CompanionBonusEntry> companionBonuses,
            List<CaravanDatabase.CaravanUpgradeEntry> caravanUpgrades,
            List<CaravanDatabase.AnimalBiomeEntry> animalBiome)
        {
            string assetPath = $"{DATABASES_FOLDER}/CaravanDatabase.asset";
            CaravanDatabase db = LoadOrCreateAsset<CaravanDatabase>(assetPath);

            db.ApplyImport(
                cartClasses,
                extraCarts,
                companionTypes,
                draftAnimals,
                upgradeLevels,
                companionQualities,
                companionBonuses,
                caravanUpgrades,
                animalBiome);

            EditorUtility.SetDirty(db);
        }
    }
}
