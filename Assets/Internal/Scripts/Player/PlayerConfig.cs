using System;
using System.Collections.Generic;
using Internal.Scripts.Npc.Core;
using UnityEngine;

namespace Internal.Scripts.Player
{
    [CreateAssetMenu(menuName = "SPJ/Player/Config", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Serializable]
        public struct StartItemEntry
        {
            [field: SerializeField] public string ItemId { get; private set; }
            [field: SerializeField] public int Count { get; private set; }
        }

        [field: SerializeField] public RoadAgentConfig RoadAgentConfig { get; private set; }
        [field: SerializeField] public string StartNodeId { get; private set; } = "N_Quanzhou";
        [field: SerializeField] public int StartMoney { get; private set; } = 200;
        [field: SerializeField] public float MaxWeightKg { get; private set; } = 100f;
        [field: SerializeField] public List<StartItemEntry> StartItems { get; private set; } = new();
    }
}
