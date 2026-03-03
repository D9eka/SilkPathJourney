using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using UnityEngine;

namespace Internal.Scripts.Npc.Encounter
{
    [CreateAssetMenu(menuName = "SPJ/Npc Encounter Settings")]
    public sealed class NpcEncounterSettings : ScriptableObject
    {
        [Min(0.1f)] public float EncounterDistance = 3f;
        [Min(1f)] public float CooldownSeconds = 30f;
        [Range(0f, 0.5f)] public float NpcTradeMarkup = 0.2f;
        [Min(1f)] public float SuppliesMarkupMultiplier = 10f;
        public List<EventData> EncounterEvents;
    }
}
