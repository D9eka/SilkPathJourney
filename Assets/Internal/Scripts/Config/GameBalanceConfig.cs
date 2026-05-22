using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using UnityEngine;

namespace Internal.Scripts.Config
{
    [Serializable]
    public struct SegmentTriggerWeights
    {
        public int Hazard;
        public int Encounter;
        public int Pickup;
    }

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
        [field: SerializeField] public float WorldUnitsPerKm { get; private set; } = 0.334f;

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
        [field: SerializeField] public int PriceTipBaseCost { get; private set; } = 20;
        [field: SerializeField] public int PriceTipCostPerHop { get; private set; } = 10;

        public int GetPriceTipCost(int distanceHops) =>
            PriceTipBaseCost + PriceTipCostPerHop * Mathf.Max(0, distanceHops);

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

        [Header("Road Hazards")]
        [field: SerializeField] public float SegmentTriggerIntervalMeters { get; private set; } = 10f;
        [field: SerializeField] public float HazardDangerMultiplier { get; private set; } = 0.5f;

        [Header("Road Pickups")]
        [field: SerializeField] public float PickupSpawnIntervalMeters { get; private set; } = 1f;
        [field: SerializeField] public float PickupVisibleRadius { get; private set; } = 15f;
        [field: SerializeField] public float PickupDespawnRadius { get; private set; } = 25f;
        [field: SerializeField] public float PickupLifetimeSeconds { get; private set; } = 30f;
        [field: SerializeField] public int MaxActivePickups { get; private set; } = 3;

        [Header("Temple")]
        [field: SerializeField] public int TemplePrayCost { get; private set; } = 10;
        [field: SerializeField] public int TempleDonateCost { get; private set; } = 50;
        [field: SerializeField] public int TempleBlessCost { get; private set; } = 30;
        [field: SerializeField] public int TempleMoraleGain { get; private set; } = 5;
        [field: SerializeField] public int TempleReputationGain { get; private set; } = 5;
        [field: SerializeField] public int TempleDangerReduction { get; private set; } = 20;

        [Header("Barracks")]
        [field: SerializeField] public int BarracksGuardCost { get; private set; } = 80;
        [field: SerializeField] public int MaxExtraCarts { get; private set; } = 3;

        [Header("Healer")]
        [field: SerializeField] public int HealerNoviceCost { get; private set; } = 10;
        [field: SerializeField] public int HealerExperiencedCost { get; private set; } = 25;
        [field: SerializeField] public int HealerMasterCost { get; private set; } = 50;

        [Header("Camp")]
        [field: SerializeField] public float CampSkillEffectPerPoint { get; private set; } = 0.001f;

        [Header("Segment Trigger Weights")]
        [field: SerializeField] public SegmentTriggerWeights SegmentWeights { get; private set; }
    }
}
