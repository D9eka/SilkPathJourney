using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Skills;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [CreateAssetMenu(menuName = "SPJ/UI/Skill Icon Catalog")]
    public class SkillIconCatalog : ScriptableObject
    {
        [SerializeField] private SkillIconEntry[] _entries;

        private Dictionary<SkillType, Sprite> _lookup;

        public Sprite Get(SkillType type)
        {
            if (_lookup == null)
                _lookup = BuildLookup();

            _lookup.TryGetValue(type, out Sprite result);
            return result;
        }

        private Dictionary<SkillType, Sprite> BuildLookup()
        {
            var lookup = new Dictionary<SkillType, Sprite>();
            if (_entries != null)
            {
                foreach (var entry in _entries)
                    lookup[entry.Type] = entry.Icon;
            }
            return lookup;
        }
    }

    [Serializable]
    public class SkillIconEntry
    {
        public SkillType Type;
        public Sprite Icon;
    }
}
