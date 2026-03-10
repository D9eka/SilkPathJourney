using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class MinLanguageProficiencyEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinLanguageProficiency
        };

        private readonly PlayerLanguageRepository _languageRepository;

        public MinLanguageProficiencyEvaluator(PlayerLanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (!Enum.TryParse(condition.Param, true, out LanguageType languageType) || languageType == LanguageType.None)
            {
                Debug.LogWarning($"[SPJ Events] Unknown language param '{condition.Param}'");
                return true;
            }

            int proficiency = (int)_languageRepository.Current.GetProficiency(languageType);
            return proficiency >= condition.Value;
        }
    }
}
