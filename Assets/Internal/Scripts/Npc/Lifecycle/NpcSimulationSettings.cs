using System.Collections.Generic;
using UnityEngine;
using Internal.Scripts.Npc.Core;

namespace Internal.Scripts.Npc.Lifecycle
{
    [CreateAssetMenu(menuName = "SPJ/Npc Simulation Settings", fileName = "NpcSimulationSettings")]
    public sealed class NpcSimulationSettings : ScriptableObject
    {
        [Min(1)] public int AgentCount = 5;
        public Vector2 SpeedRangeMetersPerSecond = new(2f, 6f);
        public List<NpcView> Prefabs = new();
        public List<Color> AvailableColors = new();
    }
}
