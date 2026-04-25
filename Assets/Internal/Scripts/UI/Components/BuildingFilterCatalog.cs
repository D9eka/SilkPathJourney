using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [CreateAssetMenu(menuName = "SPJ/UI/Building Filter Catalog")]
    public class BuildingFilterCatalog : ScriptableObject
    {
        [SerializeField] private BuildingFilterEntry[] _entries;

        private Dictionary<BuildingId, BuildingFilterEntry> _lookup;

        public BuildingFilterEntry Get(BuildingId building)
        {
            if (_lookup == null)
                _lookup = BuildLookup();

            _lookup.TryGetValue(building, out BuildingFilterEntry result);
            return result;
        }

        private Dictionary<BuildingId, BuildingFilterEntry> BuildLookup()
        {
            Dictionary<BuildingId, BuildingFilterEntry> lookup = new();
            if (_entries != null)
            {
                foreach (BuildingFilterEntry entry in _entries)
                    lookup[entry.Building] = entry;
            }
            return lookup;
        }
    }
}
