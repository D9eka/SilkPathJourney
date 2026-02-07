using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Events.Data
{
    [CreateAssetMenu(menuName = "SPJ/Events/Event Database", fileName = "EventDatabase")]
    public class EventDatabase : ScriptableObject
    {
        [field: SerializeField] public List<EventData> Events { get; private set; }

#if UNITY_EDITOR
        public void ApplyImport(List<EventData> events)
        {
            Events = events ?? new List<EventData>();
        }
#endif
    }
}
