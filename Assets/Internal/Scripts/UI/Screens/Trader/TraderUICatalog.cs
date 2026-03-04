using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Skills;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    [CreateAssetMenu(menuName = "SPJ/UI/Trader UI Catalog", fileName = "TraderUICatalog")]
    public sealed class TraderUICatalog : ScriptableObject
    {
        [Serializable]
        public struct SkillLocEntry
        {
            public SkillType Type;
            public LocalizedString Name;
            public LocalizedString Description;
        }

        [Serializable]
        public struct ProfileLocEntry
        {
            public string Id;
            public LocalizedString Header;
        }

        [SerializeField] private List<SkillLocEntry> _skills = new();
        [SerializeField] private List<ProfileLocEntry> _profileItems = new();

        public IReadOnlyList<ProfileLocEntry> ProfileItems => _profileItems;

        public void ApplyImport(List<SkillLocEntry> skills, List<ProfileLocEntry> profileItems)
        {
            _skills = skills ?? new List<SkillLocEntry>();
            _profileItems = profileItems ?? new List<ProfileLocEntry>();
        }

        public bool TryGetSkill(SkillType type, out LocalizedString name, out LocalizedString description)
        {
            foreach (SkillLocEntry entry in _skills)
            {
                if (entry.Type == type)
                {
                    name = entry.Name;
                    description = entry.Description;
                    return true;
                }
            }

            name = null;
            description = null;
            return false;
        }

        public bool TryGetProfileHeader(string id, out LocalizedString header)
        {
            foreach (ProfileLocEntry entry in _profileItems)
            {
                if (string.Equals(entry.Id, id, StringComparison.Ordinal))
                {
                    header = entry.Header;
                    return true;
                }
            }

            header = null;
            return false;
        }
    }
}
