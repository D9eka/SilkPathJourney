using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Player.Background
{
    [Serializable]
    public struct LanguageStartEntry
    {
        [field: SerializeField] public LanguageType LanguageType { get; private set; }
        [field: SerializeField] public int Level { get; private set; }

#if UNITY_EDITOR
        public LanguageStartEntry(LanguageType languageType, int level)
        {
            LanguageType = languageType;
            Level = level;
        }
#endif
    }

    [Serializable]
    public struct ItemStartEntry
    {
        [field: SerializeField] public string ItemId { get; private set; }
        [field: SerializeField] public int Count { get; private set; }

#if UNITY_EDITOR
        public ItemStartEntry(string itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }
#endif
    }

    [CreateAssetMenu(menuName = "SPJ/Player/Background", fileName = "Background")]
    public class BackgroundData : ScriptableObject
    {
        public const int MaxSkillValue = 100;

        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();
        [field: SerializeField] public bool IsUnlockedByDefault { get; private set; }
        [field: SerializeField] public int UnlockCost { get; private set; }

        [field: SerializeField] public int TradeSkill { get; private set; }
        [field: SerializeField] public int CharismaSkill { get; private set; }
        [field: SerializeField] public int SurvivalSkill { get; private set; }

        [field: SerializeField] public string RoleKey { get; private set; }
        [field: SerializeField] public BackgroundDifficulty Difficulty { get; private set; }
        [field: SerializeField] public SkillType MainSkillType { get; private set; }
        [field: SerializeField] public SkillType SecondarySkillType { get; private set; }
        [field: SerializeField] public string FeaturesKey { get; private set; }
        [field: SerializeField] public bool IsRecommended { get; private set; }

        [field: SerializeField] public List<LanguageStartEntry> StartingLanguages { get; private set; } = new();

        [field: SerializeField] public int StartingMoney { get; private set; }
        [field: SerializeField] public int StartingSupplies { get; private set; }
        [field: SerializeField] public int StartingReputation { get; private set; }
        [field: SerializeField] public int StartingMoraleBonus { get; private set; }

        [field: SerializeField] public List<ItemStartEntry> StartingItems { get; private set; } = new();

        [field: SerializeField] public string StartingCityId { get; private set; }

        public int GetSkillValue(SkillType type)
        {
            return type switch
            {
                SkillType.Trade => TradeSkill,
                SkillType.Charisma => CharismaSkill,
                SkillType.Survival => SurvivalSkill,
                _ => 0
            };
        }

        public float GetDifficultyFill()
        {
            return Difficulty switch
            {
                BackgroundDifficulty.Easy => 0.33f,
                BackgroundDifficulty.Normal => 0.66f,
                BackgroundDifficulty.Hard => 1.0f,
                _ => 0f
            };
        }

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            LocalizedString name,
            LocalizedString description,
            bool isUnlockedByDefault,
            int unlockCost,
            int tradeSkill,
            int charismaSkill,
            int survivalSkill,
            string roleKey,
            BackgroundDifficulty difficulty,
            SkillType mainSkillType,
            SkillType secondarySkillType,
            string featuresKey,
            bool isRecommended,
            List<LanguageStartEntry> startingLanguages,
            int startingMoney,
            int startingSupplies,
            int startingReputation,
            int startingMoraleBonus,
            List<ItemStartEntry> startingItems,
            string startingCityId)
        {
            Id = id;
            Name = name;
            Description = description;
            IsUnlockedByDefault = isUnlockedByDefault;
            UnlockCost = unlockCost;
            TradeSkill = tradeSkill;
            CharismaSkill = charismaSkill;
            SurvivalSkill = survivalSkill;
            RoleKey = roleKey;
            Difficulty = difficulty;
            MainSkillType = mainSkillType;
            SecondarySkillType = secondarySkillType;
            FeaturesKey = featuresKey;
            IsRecommended = isRecommended;
            StartingLanguages = startingLanguages ?? new List<LanguageStartEntry>();
            StartingMoney = startingMoney;
            StartingSupplies = startingSupplies;
            StartingReputation = startingReputation;
            StartingMoraleBonus = startingMoraleBonus;
            StartingItems = startingItems ?? new List<ItemStartEntry>();
            StartingCityId = startingCityId ?? string.Empty;
        }
#endif
    }
}
