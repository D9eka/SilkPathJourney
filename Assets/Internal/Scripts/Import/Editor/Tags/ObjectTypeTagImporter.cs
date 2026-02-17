using System;
using System.Collections.Generic;
using Internal.Scripts.World.ObjectType;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Import.Editor.Tags
{
    public sealed class ObjectTypeTagImporter : AssetPostprocessor
    {
        private const string KEY = "object_type";

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
            if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "none", StringComparison.OrdinalIgnoreCase))
                return;

            ObjectTypeTag comp = go.GetComponent<ObjectTypeTag>();
            if (comp == null) comp = go.AddComponent<ObjectTypeTag>();
            comp.EditorSetType(raw.Trim().ToLowerInvariant());
            EditorUtility.SetDirty(comp);

            Debug.Log($"[SPJ] object_type='{raw}' on '{go.name}' ({assetPath})", go);
        }

        private static string TryGet(string[] names, object[] values, string key)
        {
            for (int i = 0; i < names.Length; i++)
                if (string.Equals(names[i], key, StringComparison.OrdinalIgnoreCase))
                    return values[i]?.ToString();
            return null;
        }
    }
}
