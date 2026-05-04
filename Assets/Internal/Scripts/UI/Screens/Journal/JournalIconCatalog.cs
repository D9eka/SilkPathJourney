using System;
using System.Collections.Generic;
using Internal.Scripts.Journal;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Journal
{
    [CreateAssetMenu(menuName = "SPJ/UI/Journal Icon Catalog", fileName = "JournalIconCatalog")]
    public class JournalIconCatalog : ScriptableObject
    {
        [SerializeField] private JournalIconEntry[] _entries;

        private Dictionary<JournalEntryType, Sprite> _lookup;

        public Sprite Get(JournalEntryType type)
        {
            if (_lookup == null)
                _lookup = BuildLookup();

            _lookup.TryGetValue(type, out Sprite result);
            return result;
        }

        private Dictionary<JournalEntryType, Sprite> BuildLookup()
        {
            var lookup = new Dictionary<JournalEntryType, Sprite>();
            if (_entries != null)
            {
                foreach (var entry in _entries)
                    lookup[entry.Type] = entry.Icon;
            }
            return lookup;
        }
    }

    [Serializable]
    public class JournalIconEntry
    {
        public JournalEntryType Type;
        public Sprite Icon;
    }
}
