using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Localization;
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

        [Serializable]
        public struct LanguageLocEntry
        {
            public LanguageType Type;
            public LocalizedString Name;
            public LocalizedString Description;
        }

        [SerializeField] private List<SkillLocEntry> _skills = new();
        [SerializeField] private List<LanguageLocEntry> _languages = new();
        [SerializeField] private List<ProfileLocEntry> _profileItems = new();

        public IReadOnlyList<ProfileLocEntry> ProfileItems => _profileItems;

        public void ApplyImport(List<SkillLocEntry> skills, List<LanguageLocEntry> languages,
            List<ProfileLocEntry> profileItems)
        {
            _skills = skills ?? new List<SkillLocEntry>();
            _languages = languages ?? new List<LanguageLocEntry>();
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

        public string GetSkillName(SkillType type)
        {
            if (TryGetSkill(type, out LocalizedString name, out _))
                return LocalizationService.ResolveString(name, type.ToString(), "SkillName");
            return type.ToString();
        }

        public bool TryGetLanguage(LanguageType type, out LocalizedString name, out LocalizedString description)
        {
            foreach (LanguageLocEntry entry in _languages)
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

        public string GetLanguageName(LanguageType type)
        {
            if (TryGetLanguage(type, out LocalizedString name, out _))
                return LocalizationService.ResolveString(name, type.ToString(), "LanguageName");
            return type.ToString();
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
