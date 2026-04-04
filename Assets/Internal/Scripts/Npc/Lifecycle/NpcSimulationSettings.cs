using System.Collections.Generic;
using UnityEngine;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Npc.Behavior;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Lifecycle
{
    [CreateAssetMenu(menuName = "SPJ/NPC/Simulation Settings", fileName = "NpcSimulationSettings")]
    public sealed class NpcSimulationSettings : ScriptableObject
    {
        [Min(1)] public int AgentCount = 5;

        public Vector2 SpeedRangeMetersPerDay = new(2f, 6f);

        public List<NpcView> Prefabs = new();
        public List<Color> AvailableColors = new();

        [Header("Trading")]
        public Vector2Int MoneyRange = new(300, 700);
        public Vector2 CapacityRange = new(150f, 350f);
        [Range(0f, 1f)] public float BuyBudgetFraction = 0.7f;
        [Min(1)] public int MaxBuyItemTypes = 3;
        [Min(1)] public int SuppliesPerDay = 1;
        [Min(1)] public int StartingSupplies = 5;
        [Min(0)] public int ExtraSuppliesDays = 1;
        [Range(0f, 1f)] public float StarvationSurvivalChance = 0.5f;
        [Min(1f)] public float RoadWindingFactor = 1.4f;

        [Header("Behavior Profiles")]
        [field: SerializeField] public NpcBehaviorProfile DefaultBehaviorProfile { get; private set; }
        [field: SerializeField] public List<NpcBehaviorProfile> BehaviorProfiles { get; private set; } = new();

        [Header("Archetypes")]
        [field: SerializeField] public List<NpcArchetypeDefinition> Archetypes { get; private set; } = new();
        [field: SerializeField] public List<ArchetypeCityAffinity> CityAffinityTable { get; private set; } = new();

        [Header("Experience")]
        [field: SerializeField] public int NoviceCount { get; private set; } = 6;
        [field: SerializeField] public int ExperiencedCount { get; private set; } = 10;
        [field: SerializeField] public int MasterCount { get; private set; } = 4;

        [Header("Smart Trading")]
        [field: SerializeField] public int MinBuyItemPrice { get; private set; } = 10;
        [field: SerializeField] public float ScarcityThreshold { get; private set; } = 1.28f;
        [field: SerializeField] public float TransportCostPerDay { get; private set; } = 0.3f;
        [field: SerializeField] public int ForceSellDays { get; private set; } = 200;
        [field: SerializeField] public int ForceSellMoneyThreshold { get; private set; } = 150;
        [field: SerializeField] public float ForceSellCapacityFraction { get; private set; } = 0.4f;
        [field: SerializeField] public float HoldThresholdNovice { get; private set; } = 0.90f;
        [field: SerializeField] public float HoldThresholdExperienced { get; private set; } = 0.85f;
        [field: SerializeField] public float HoldThresholdMaster { get; private set; } = 0.80f;
        [field: SerializeField] public float[] BudgetShares { get; private set; } = { 0.60f, 0.25f, 0.15f };
        [field: SerializeField] public int NoPathFallbackDays { get; private set; } = 5;

        [Header("Modifier Reactions")]
        [field: SerializeField] public float FestivalScoreBonus { get; private set; } = 0.4f;
        [field: SerializeField] public float BazaarFestivalScoreBonus { get; private set; } = 0.6f;
        [field: SerializeField] public float BanditsScorePenalty { get; private set; } = 0.7f;
        [field: SerializeField] public float DroughtBazaarBonus { get; private set; } = 0.5f;
        [field: SerializeField] public float HighTaxesPenalty { get; private set; } = 0.3f;
        [field: SerializeField] public float CaravanArrivalBonus { get; private set; } = 0.2f;

        [Header("Knowledge")]
        [field: SerializeField] public int BaseKnowledgeDuration { get; private set; } = 30;
        [field: SerializeField] public float NoviceKnowledgeMult { get; private set; } = 1.0f;
        [field: SerializeField] public float ExperiencedKnowledgeMult { get; private set; } = 1.5f;
        [field: SerializeField] public float MasterKnowledgeMult { get; private set; } = 2.0f;

        [Header("Foraging")]
        [field: SerializeField] public int ForageThreshold { get; private set; } = 3;
        [field: SerializeField] public int ForageAmount { get; private set; } = 7;
        [field: SerializeField] public int ForageCooldownDays { get; private set; } = 5;

        [Header("Guild Credit")]
        [field: SerializeField] public int SurvivalMoneyThreshold { get; private set; } = 100;
        [field: SerializeField] public int CreditMoneyThreshold { get; private set; } = 50;
        [field: SerializeField] public int CreditAmount { get; private set; } = 300;
        [field: SerializeField] public float DebtRepaymentFraction { get; private set; } = 0.3f;
        [field: SerializeField] public int MaxDebtDays { get; private set; } = 30;

        private void Reset()
        {
            var savedPrefabs = Prefabs;
            var savedColors = AvailableColors;

            Archetypes = new List<NpcArchetypeDefinition>
            {
                new() { Archetype = NpcArchetype.SilkMerchant, SpawnCount = 5,
                        PreferredCategories = new[] { ItemType.Luxury, ItemType.Exotic },
                        MoneyRange = new Vector2Int(500, 700), CapacityRange = new Vector2(150, 200),
                        MinProfitThreshold = 1.15f, WarEntryChance = 0.2f, MaxItemWeight = 0, MinItemPrice = 0,
                        RouteDurationBias = 0.10f },
                new() { Archetype = NpcArchetype.SteppeHerder, SpawnCount = 4,
                        PreferredCategories = new[] { ItemType.Raw, ItemType.Animals },
                        MoneyRange = new Vector2Int(400, 600), CapacityRange = new Vector2(250, 350),
                        MinProfitThreshold = 1.08f, WarEntryChance = 0, MaxItemWeight = 0, MinItemPrice = 0,
                        RouteDurationBias = -0.10f },
                new() { Archetype = NpcArchetype.BazaarTrader, SpawnCount = 6,
                        PreferredCategories = new[] { ItemType.Craft, ItemType.Food },
                        MoneyRange = new Vector2Int(300, 500), CapacityRange = new Vector2(200, 300),
                        MinProfitThreshold = 1.10f, WarEntryChance = 0, MaxItemWeight = 0, MinItemPrice = 0,
                        RouteDurationBias = -0.35f },
                new() { Archetype = NpcArchetype.PortAgent, SpawnCount = 3,
                        PreferredCategories = new[] { ItemType.Exotic, ItemType.Luxury },
                        MoneyRange = new Vector2Int(400, 650), CapacityRange = new Vector2(200, 250),
                        MinProfitThreshold = 1.12f, WarEntryChance = 0, MaxItemWeight = 0, MinItemPrice = 0,
                        RouteDurationBias = 0.15f },
                new() { Archetype = NpcArchetype.Adventurer, SpawnCount = 2,
                        PreferredCategories = new[] { ItemType.Luxury, ItemType.Exotic },
                        MoneyRange = new Vector2Int(450, 700), CapacityRange = new Vector2(150, 200),
                        MinProfitThreshold = 1.20f, WarEntryChance = 0.2f, MaxItemWeight = 3, MinItemPrice = 40,
                        RouteDurationBias = 0.20f },
            };

            CityAffinityTable = new List<ArchetypeCityAffinity>();
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.Capital,       2.5f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.TradeHub,      1.3f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.BorderCity,    0.9f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.CraftCity,     0.8f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.ResourceCity,  0.4f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.MarketTown,    0.7f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.OasisStop,     0.3f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.PortCity,      1.2f);
            AddAffinity(NpcArchetype.SilkMerchant,  CityType.HolyCity,      2.0f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.Capital,       0.5f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.TradeHub,      1.0f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.BorderCity,    1.8f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.CraftCity,     1.1f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.ResourceCity,  2.0f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.MarketTown,    1.0f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.OasisStop,     0.8f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.PortCity,      0.7f);
            AddAffinity(NpcArchetype.SteppeHerder,  CityType.HolyCity,      0.4f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.Capital,       1.1f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.TradeHub,      2.5f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.BorderCity,    0.9f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.CraftCity,     1.3f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.ResourceCity,  0.5f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.MarketTown,    2.0f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.OasisStop,     0.8f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.PortCity,      0.9f);
            AddAffinity(NpcArchetype.BazaarTrader,  CityType.HolyCity,      1.0f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.Capital,       2.0f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.TradeHub,      1.1f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.BorderCity,    0.8f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.CraftCity,     0.7f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.ResourceCity,  0.7f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.MarketTown,    0.6f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.OasisStop,     0.3f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.PortCity,      2.5f);
            AddAffinity(NpcArchetype.PortAgent,     CityType.HolyCity,      0.6f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.Capital,       1.2f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.TradeHub,      0.9f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.BorderCity,    2.0f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.CraftCity,     0.8f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.ResourceCity,  0.8f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.MarketTown,    0.7f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.OasisStop,     0.4f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.PortCity,      1.0f);
            AddAffinity(NpcArchetype.Adventurer,    CityType.HolyCity,      1.4f);

            Prefabs = savedPrefabs;
            AvailableColors = savedColors;
        }

        public void ApplyBehaviorProfilesImport(NpcBehaviorProfile defaultProfile, List<NpcBehaviorProfile> profiles)
        {
            DefaultBehaviorProfile = defaultProfile;
            BehaviorProfiles = profiles ?? BehaviorProfiles;
        }

        public NpcArchetypeDefinition GetArchetypeDefinition(NpcArchetype archetype)
        {
            foreach (NpcArchetypeDefinition def in Archetypes)
            {
                if (def.Archetype == archetype)
                    return def;
            }
            return Archetypes.Count > 0 ? Archetypes[0] : default;
        }

        private void AddAffinity(NpcArchetype archetype, CityType cityType, float affinity)
        {
            CityAffinityTable.Add(new ArchetypeCityAffinity
            {
                Archetype = archetype,
                CityType = cityType,
                Affinity = affinity
            });
        }
    }
}
