using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.Npc.Names
{
    [CreateAssetMenu(menuName = "SPJ/NPC/Name Database", fileName = "NameDatabase")]
    public sealed class NameDatabase : ScriptableObject
    {
        [SerializeField] private List<NameEntry> _entries = new();

        public IReadOnlyList<NameEntry> Entries => _entries;

        public List<NameEntry> GetNamesByCulture(CultureId culture)
        {
            List<NameEntry> result = new();
            foreach (NameEntry entry in _entries)
            {
                if (entry.Culture == culture)
                    result.Add(entry);
            }
            return result;
        }

        public NameEntry GetRandom(CultureId culture)
        {
            List<NameEntry> filtered = GetNamesByCulture(culture);
            if (filtered.Count == 0)
                return GetRandom();

            return filtered[Random.Range(0, filtered.Count)];
        }

        public NameEntry GetRandom()
        {
            if (_entries.Count == 0)
                return null;

            return _entries[Random.Range(0, _entries.Count)];
        }

#if UNITY_EDITOR
        public void ApplyImport(List<NameEntry> entries)
        {
            _entries = entries ?? new List<NameEntry>();
        }
#endif
    }
}
