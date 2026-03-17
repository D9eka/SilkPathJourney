using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class CartClassesTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Caravan";
        private const string LOC_TABLE = "Caravan";

        public static List<CartClassData> Import(Dictionary<string, CartClass> classMap)
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("cart_classes.csv"));
            List<CartClassData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "class_id");
            int speedIdx = FindColumnIndex(header, "speed_km_day");
            int capIdx = FindColumnIndex(header, "capacity");
            int durIdx = FindColumnIndex(header, "durability");
            int animalIdx = FindColumnIndex(header, "animal_count");
            int locIdx = FindColumnIndex(header, "loc_link");
            int descLocIdx = FindColumnIndex(header, "desc_loc_link");
            if (idIdx < 0 || speedIdx < 0 || capIdx < 0 || durIdx < 0 || animalIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in cart_classes.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CartClass classType = TryLookup(classMap, id, CartClass.Unknown, "cart_classes.csv", i + 1, "class_id");

                TryParseFloat(GetField(rows[i], speedIdx), out float speed);
                TryParseFloat(GetField(rows[i], capIdx), out float capacity);
                TryParseFloat(GetField(rows[i], durIdx), out float durability);
                TryParseInt(GetField(rows[i], animalIdx), out int animalCount);

                CartClassData asset = LoadOrCreateAsset<CartClassData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(id, classType, speed, capacity, durability, animalCount,
                    MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE),
                    MakeLocalizedString(descLocIdx >= 0 ? GetField(rows[i], descLocIdx).Trim() : "", LOC_TABLE));

                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }
    }
}
