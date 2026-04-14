using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using UnityEngine;

namespace Internal.Scripts.Config
{
    [CreateAssetMenu(menuName = "SPJ/Game Balance Config", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Day Progression")]
        [field: SerializeField] public float SecondsPerDay { get; private set; } = 20f;

        [Header("Events")]
        [field: SerializeField] public int DaysBetweenMajorEvents { get; private set; } = 5;
        [field: SerializeField] public int DaysBetweenMinorEvents { get; private set; } = 2;
        [field: SerializeField] public float ToastDisplayDuration { get; private set; } = 4f;

        [Header("Resource Limits")]
        [field: SerializeField] public float MaxFood { get; private set; } = 100f;
        [field: SerializeField] public float MaxDanger { get; private set; } = 100f;

        [Header("Language School")]
        [field: SerializeField] public int SchoolCostBasic { get; private set; } = 40;
        [field: SerializeField] public int SchoolDaysBasic { get; private set; } = 5;
        [field: SerializeField] public int SchoolCostConversational { get; private set; } = 100;
        [field: SerializeField] public int SchoolDaysConversational { get; private set; } = 10;
        [field: SerializeField] public int SchoolCostFluent { get; private set; } = 150;
        [field: SerializeField] public int SchoolDaysFluent { get; private set; } = 20;

        [Header("Caravan Speed")]
        [field: SerializeField] public float WorldUnitsPerKm { get; private set; } = 0.167f;

        [Header("Caravan Repair")]
        [field: SerializeField] public int DefaultRepairPercent { get; private set; } = 5;
        [field: SerializeField] public int RepairCostPerPercent { get; private set; } = 2;

        [Header("Caravansary Services")]
        [field: SerializeField] public int RestCost { get; private set; } = 20;

        [Header("Skills")]
        [field: SerializeField] public int MaxSkill { get; private set; } = 100;
        [field: SerializeField] public float TradePriceDivisor { get; private set; } = 500f;
        [field: SerializeField] public float SkillBonusPerPoint { get; private set; } = 0.0045f;
        [field: SerializeField] public float SkillChanceCap { get; private set; } = 0.95f;

        [Header("World Modifiers")]
        [field: SerializeField] public float ModifierDurationMultiplier { get; private set; } = 1f;
        [field: SerializeField] public int StalenessActualDays { get; private set; } = 20;
        [field: SerializeField] public int StalenessStaleDays { get; private set; } = 40;

        [Header("Reputation")]
        [field: SerializeField] public int StartingReputation { get; private set; } = 50;

        [Header("Rumors")]
        [field: SerializeField] public int RumorCost { get; private set; } = 10;
        [field: SerializeField] public int RumorRadius { get; private set; } = 2;

        [Header("Exoticity")]
        [field: SerializeField] public float ExoticityPerStep { get; private set; } = 0.15f;

        [Header("Tariff")]
        [field: SerializeField] public List<ThresholdModifierRule> TariffReputationRules { get; private set; } = new()
        {
            new() { Threshold = 20, Modifier = 1.3f, Comparison = ComparisonType.Below },
            new() { Threshold = 40, Modifier = 1.1f, Comparison = ComparisonType.Below },
            new() { Threshold = 60, Modifier = 1.0f, Comparison = ComparisonType.Below },
            new() { Threshold = 80, Modifier = 0.8f, Comparison = ComparisonType.Below },
        };
        [field: SerializeField] public float TariffDefaultReputationModifier { get; private set; } = 0.5f;
        [Header("Camp")]
        [field: SerializeField] public float CampSkillEffectPerPoint { get; private set; } = 0.001f;
    }
}
