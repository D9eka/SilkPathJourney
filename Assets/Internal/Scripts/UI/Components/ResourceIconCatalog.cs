using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [CreateAssetMenu(menuName = "SPJ/UI/Resource Icon Catalog")]
    public class ResourceIconCatalog : ScriptableObject
    {
        [SerializeField] private ResourceEntry[] _resources;

        private Dictionary<ResourceType, ResourceEntry> _lookup;

        public ResourceEntry Get(ResourceType type)
        {
            if (_lookup == null)
            {
                _lookup = BuildLookup();
            }

            _lookup.TryGetValue(type, out ResourceEntry result);
            return result;
        }
        
        private Dictionary<ResourceType, ResourceEntry> BuildLookup()
        {
            Dictionary<ResourceType, ResourceEntry> lookup = new();
            if (_resources != null)
            {
                foreach (ResourceEntry entry in _resources)
                    lookup[entry.Type] = entry;
            }
            return lookup;
        }
    }
}
