using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.Npc.Data
{
    public enum NpcArchetype
    {
        BazaarTrader = 0,
        SilkMerchant = 1,
        SteppeHerder = 2,
        PortAgent = 3,
        Adventurer = 4
    }

    public enum NpcExperienceLevel
    {
        Novice = 0,
        Experienced = 1,
        Master = 2
    }

    [Serializable]
    public struct NpcArchetypeDefinition
    {
        [field: SerializeField] public NpcArchetype Archetype { get; set; }
        [field: SerializeField] public int SpawnCount { get; set; }
        [field: SerializeField] public ItemType[] PreferredCategories { get; set; }
        [field: SerializeField] public Vector2Int MoneyRange { get; set; }
        [field: SerializeField] public Vector2 CapacityRange { get; set; }
        [field: SerializeField] public float MinProfitThreshold { get; set; }
        [field: SerializeField] public float WarEntryChance { get; set; }
        [field: SerializeField] public float MaxItemWeight { get; set; }
        [field: SerializeField] public float MinItemPrice { get; set; }
        [field: SerializeField] public float RouteDurationBias { get; set; }
    }

    [Serializable]
    public struct ArchetypeCityAffinity
    {
        public NpcArchetype Archetype;
        public CityType CityType;
        public float Affinity;
    }

    [Serializable]
    public struct PurchaseRecord
    {
        public string ItemId;
        public int PricePerUnit;
        public int DayBought;
        public CultureId OriginCulture;
    }

    [Serializable]
    public struct KnownCityModifier
    {
        public string CityId;
        public string ModifierId;
        public int LearnedDay;
    }

    [Serializable]
    public class NpcKnowledgeState
    {
        public List<KnownCityModifier> Entries = new();
    }
}
