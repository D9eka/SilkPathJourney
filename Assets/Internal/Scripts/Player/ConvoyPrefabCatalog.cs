using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using UnityEngine;

namespace Internal.Scripts.Player
{
    [CreateAssetMenu(menuName = "SPJ/Convoy Prefab Catalog")]
    public sealed class ConvoyPrefabCatalog : ScriptableObject
    {
        [Serializable]
        public struct MainCartEntry
        {
            public CartClass Class;
            public GameObject Prefab;
        }

        [Serializable]
        public struct ExtraCartEntry
        {
            public ExtraCartType Type;
            public GameObject Prefab;
        }

        [Serializable]
        public struct AnimalPrefabEntry
        {
            public DraftAnimalType Type;
            public GameObject Prefab;
            public float Scale;
            public Vector3 Offset;
        }

        [SerializeField] private MainCartEntry[] _mainCarts;
        [SerializeField] private ExtraCartEntry[] _extraCarts;
        [SerializeField] private AnimalPrefabEntry[] _animals;

        private Dictionary<CartClass, GameObject> _mainCartLookup;
        private Dictionary<ExtraCartType, GameObject> _extraCartLookup;
        private Dictionary<DraftAnimalType, AnimalPrefabEntry> _animalLookup;

        public GameObject GetMainCart(CartClass cartClass)
        {
            if (_mainCartLookup == null) BuildMainCartLookup();
            _mainCartLookup.TryGetValue(cartClass, out var prefab);
            return prefab;
        }

        public GameObject GetExtraCart(ExtraCartType extraType)
        {
            if (_extraCartLookup == null) BuildExtraCartLookup();
            _extraCartLookup.TryGetValue(extraType, out var prefab);
            return prefab;
        }

        public AnimalPrefabEntry GetAnimal(DraftAnimalType type)
        {
            if (_animalLookup == null) BuildAnimalLookup();
            _animalLookup.TryGetValue(type, out var entry);
            return entry;
        }

        private void BuildMainCartLookup()
        {
            _mainCartLookup = new Dictionary<CartClass, GameObject>();
            if (_mainCarts != null)
                foreach (var entry in _mainCarts)
                    if (entry.Prefab != null)
                        _mainCartLookup[entry.Class] = entry.Prefab;
        }

        private void BuildExtraCartLookup()
        {
            _extraCartLookup = new Dictionary<ExtraCartType, GameObject>();
            if (_extraCarts != null)
                foreach (var entry in _extraCarts)
                    if (entry.Prefab != null)
                        _extraCartLookup[entry.Type] = entry.Prefab;
        }

        private void BuildAnimalLookup()
        {
            _animalLookup = new Dictionary<DraftAnimalType, AnimalPrefabEntry>();
            if (_animals != null)
                foreach (var entry in _animals)
                    if (entry.Prefab != null)
                        _animalLookup[entry.Type] = entry;
        }
    }
}
