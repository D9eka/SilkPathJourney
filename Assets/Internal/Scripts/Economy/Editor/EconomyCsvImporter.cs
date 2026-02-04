using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Economy.Editor
{
    public static class EconomyCsvImporter
    {
        [MenuItem("SPJ/Import/Economy/Phase A - Generate Enums")]
        private static void GenerateEnums()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Unity is compiling. Try again after compilation finishes.");
                return;
            }

            bool changed = EnumGenerator.GenerateEconomyEnums();
            if (changed)
                Debug.Log("[SPJ] Economy enums generated. Unity will recompile if needed.");
            else
                Debug.Log("[SPJ] Economy enums are up to date.");
        }

        [MenuItem("SPJ/Import/Economy/Phase B - Import Data")]
        private static void ImportData()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Unity is compiling. Try again after compilation finishes.");
                return;
            }

            EconomyDataImporter.ImportAll();
        }
    }
}
