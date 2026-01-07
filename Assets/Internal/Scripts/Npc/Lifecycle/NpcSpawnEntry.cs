using System;
using UnityEngine;
using Internal.Scripts.Npc.Core;

namespace Internal.Scripts.Npc.Lifecycle
{
    [Serializable]
    public sealed class NpcSpawnEntry
    {
        [field: SerializeField] public NpcView View { get; private set; }
        [field: SerializeField] public string StartNodeId { get; private set; }
        [field: SerializeField] public string DestinationNodeId { get; private set; }
        [field: SerializeField] public NpcConfig Config { get; private set; } = new NpcConfig();
        [field: SerializeField] public bool AutoStart { get; private set; } = true;
    }
}
