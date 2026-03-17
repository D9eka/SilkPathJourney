using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class AnimalBiomeTable
    {
        public static List<CaravanDatabase.AnimalBiomeEntry> Read(
            Dictionary<string, DraftAnimalType> animalMap,
            Dictionary<string, Biome> biomeMap)
        {
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("animal_biome_compatibility.csv"));
            if (rows == null)
                return new List<CaravanDatabase.AnimalBiomeEntry>();

            string[] header = rows[0];
            int animalIdx = FindColumnIndex(header, "animal_id");
            int biomeIdx = FindColumnIndex(header, "biome_id");
            int compatIdx = FindColumnIndex(header, "compatibility");
            if (animalIdx < 0 || biomeIdx < 0 || compatIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in animal_biome_compatibility.csv");
                return new List<CaravanDatabase.AnimalBiomeEntry>();
            }

            List<CaravanDatabase.AnimalBiomeEntry> result = new();
            for (int i = 1; i < rows.Count; i++)
            {
                string animalId = GetField(rows[i], animalIdx).Trim();
                if (string.IsNullOrWhiteSpace(animalId))
                    continue;

                DraftAnimalType animal = TryLookup(animalMap, animalId, DraftAnimalType.Unknown,
                    "animal_biome_compatibility.csv", i + 1, "animal_id");

                string biomeId = GetField(rows[i], biomeIdx).Trim();
                Biome biome = TryLookup(biomeMap, biomeId, Biome.Unknown,
                    "animal_biome_compatibility.csv", i + 1, "biome_id");

                string compatibility = GetField(rows[i], compatIdx).Trim();

                result.Add(new CaravanDatabase.AnimalBiomeEntry
                {
                    Animal = animal,
                    Biome = biome,
                    Compatibility = compatibility
                });
            }

            return result;
        }
    }
}
