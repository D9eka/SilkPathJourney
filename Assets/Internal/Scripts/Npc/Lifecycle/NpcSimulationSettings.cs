using System.Collections.Generic;
using UnityEngine;
using Internal.Scripts.Npc.Core;
using UnityEngine.Serialization;

namespace Internal.Scripts.Npc.Lifecycle
{
    [CreateAssetMenu(menuName = "SPJ/Npc Simulation Settings", fileName = "NpcSimulationSettings")]
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
        [Min(0)] public int ExtraSuppliesDays = 2;
        [Range(0f, 1f)] public float StarvationSurvivalChance = 0.5f;
        [Min(1f)] public float RoadWindingFactor = 1.4f;
    }
}
