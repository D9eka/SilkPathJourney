using System;
using System.Collections.Generic;
using Internal.Scripts.World.State;
using Internal.Scripts.World.VisualObjects;
using UnityEditor;
using UnityEngine;
namespace Internal.Scripts.World.Editor.TagsImporter
{
    public sealed class WorldTypeTagImporter : AssetPostprocessor
    {
        private const string KEY = "world_type";

        public override uint GetVersion() => 1;

        private void OnPreprocessModel()
        {
            ModelImporter importer = (ModelImporter)assetImporter;

            List<string> list = new List<string>(importer.extraUserProperties ?? Array.Empty<string>());
            if (!list.Exists(x => string.Equals(x, KEY, StringComparison.OrdinalIgnoreCase)))
            {
                list.Add(KEY);
                importer.extraUserProperties = list.ToArray();
            }
        }

        private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propNames, object[] values)
        {
            string raw = TryGet(propNames, values, KEY);
            if (string.IsNullOrWhiteSpace(raw))
                return;

            WorldDetailLevel tag = Parse(raw);

            MonoBehVisualObject comp = go.GetComponent<MonoBehVisualObject>();
            if (comp == null) comp = go.AddComponent<MonoBehVisualObject>();
            comp.EditorSetViewMode(tag);
            EditorUtility.SetDirty(comp);

            Debug.Log($"[SPJ] world_type='{raw}' -> {tag} on '{go.name}' ({assetPath})", go);
        }

        private static string TryGet(string[] names, object[] values, string key)
        {
            for (int i = 0; i < names.Length; i++)
                if (string.Equals(names[i], key, StringComparison.OrdinalIgnoreCase))
                    return values[i]?.ToString();
            return null;
        }

        private static WorldDetailLevel Parse(string s)
        {
            s = s.Trim().ToLowerInvariant();
            return s switch
            {
                "detailed" => WorldDetailLevel.Detailed,
                "simple"   => WorldDetailLevel.Simplified,
                _          => WorldDetailLevel.Both
            };
        }
    }
}
