using System;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.Economy.Buildings
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Culture Building Catalog", fileName = "CultureBuildingCatalog")]
    public class CultureBuildingCatalog : ScriptableObject
    {
        public const string AssetPath = "Assets/Internal/Data/Generated/CultureBuildingCatalog.asset";

        [Serializable]
        public struct CultureSet
        {
            public CultureId Culture;
            public GameObject[] Prefabs;
        }

        [field: SerializeField] public CultureSet[] Sets { get; private set; } = Array.Empty<CultureSet>();

        public GameObject[] GetPrefabs(CultureId culture)
        {
            foreach (var s in Sets)
                if (s.Culture == culture) return s.Prefabs;
            return Sets.Length > 0 ? Sets[0].Prefabs : Array.Empty<GameObject>();
        }
    }
}
