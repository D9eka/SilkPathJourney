using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Languages.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [CreateAssetMenu(menuName = "SPJ/UI/Language Icon Catalog")]
    public class LanguageIconCatalog : ScriptableObject
    {
        [SerializeField] private LanguageIconEntry[] _entries;

        private Dictionary<LanguageType, Sprite> _lookup;

        public Sprite Get(LanguageType type)
        {
            if (_lookup == null)
                _lookup = BuildLookup();

            _lookup.TryGetValue(type, out Sprite result);
            return result;
        }

        private Dictionary<LanguageType, Sprite> BuildLookup()
        {
            var lookup = new Dictionary<LanguageType, Sprite>();
            if (_entries != null)
            {
                foreach (var entry in _entries)
                    lookup[entry.Type] = entry.Icon;
            }
            return lookup;
        }
    }

    [Serializable]
    public class LanguageIconEntry
    {
        public LanguageType Type;
        public Sprite Icon;
    }
}
