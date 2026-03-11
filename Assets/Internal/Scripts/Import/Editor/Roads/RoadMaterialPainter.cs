using System;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Internal.Scripts.Import.Editor.Roads
{
    public static class RoadMaterialPainter
    {
        private const string MaterialsFolder = ImportHelpers.GENERATED_DATA_FOLDER + "/Materials";
        private static readonly Color RoadColor = new(0.15f, 0.12f, 0.10f);

        [MenuItem("SPJ/Import/Roads")]
        public static void PaintRoadMaterials()
        {
            Material mat = GetOrCreateRoadMaterial();
            MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

            int painted = 0;
            foreach (MeshRenderer renderer in renderers)
            {
                string normalized = NormalizeRoadName(renderer.gameObject.name);
                if (normalized == null)
                    continue;

                Undo.RecordObject(renderer, "Paint Road Material");
                renderer.sharedMaterial = mat;
                painted++;
            }

            if (painted > 0)
                AssetDatabase.SaveAssets();

            Debug.Log($"[SPJ] Road materials painted: {painted} roads.");
        }

        private static string NormalizeRoadName(string name)
        {
            if (name.EndsWith("_EXPORT", StringComparison.OrdinalIgnoreCase))
                name = name[..^"_EXPORT".Length];

            if (!name.StartsWith("R_", StringComparison.OrdinalIgnoreCase))
                return null;

            return name;
        }

        private static Material GetOrCreateRoadMaterial()
        {
            const string matName = "M_Road";
            string matPath = $"{MaterialsFolder}/{matName}.mat";

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) return existing;

            ImportHelpers.EnsureAssetFolder(MaterialsFolder);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Standard");

            var mat = new Material(shader) { name = matName };
            mat.SetColor("_BaseColor", RoadColor);
            mat.SetColor("_Color", RoadColor);
            mat.SetFloat("_Smoothness", 0.1f);

            AssetDatabase.CreateAsset(mat, matPath);
            return mat;
        }
    }
}
