using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities
{
    [CreateAssetMenu(menuName = "SPJ/Culture Adjacency Data", fileName = "CultureAdjacencyData")]
    public sealed class CultureAdjacencyData : ScriptableObject
    {
        [Serializable]
        public struct CulturePair
        {
            public CultureId A;
            public CultureId B;
        }

        [field: SerializeField] public List<CulturePair> Adjacencies { get; private set; } = new();

#if UNITY_EDITOR
        public void SetAdjacencies(List<CulturePair> pairs)
        {
            Adjacencies = pairs ?? new List<CulturePair>();
        }
#endif
    }
}
