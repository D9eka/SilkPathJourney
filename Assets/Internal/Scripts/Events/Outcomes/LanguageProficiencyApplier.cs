using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class LanguageProficiencyApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.ChangeLanguageProficiency
        };

        private readonly PlayerLanguageRepository _languageRepository;

        public LanguageProficiencyApplier(PlayerLanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            if (!Enum.TryParse(entry.Param, true, out LanguageType languageType) || languageType == LanguageType.None)
            {
                Debug.LogWarning($"[SPJ Events] Unknown language param '{entry.Param}'");
                return;
            }

            var proficiency = (LanguageProficiency)Mathf.Clamp(
                Mathf.RoundToInt(entry.Value), 0, (int)LanguageProficiency.Native);
            var current = _languageRepository.Current.GetProficiency(languageType);
            if (proficiency > current)
                _languageRepository.SetLanguage(languageType, proficiency);
        }
    }
}
