using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Internal.Scripts.Events.Data
{
    [CreateAssetMenu(menuName = "SPJ/Events/Event Database", fileName = "EventDatabase")]
    public class EventDatabase : ScriptableObject
    {
        [field: SerializeField] public List<EventData> Events { get; private set; }

        private Dictionary<string, EventData> _byId;

        public EventData GetById(string id)
        {
            if (Events == null || Events.Count == 0) return null;
            _byId ??= Events
                .Where(e => e != null && !string.IsNullOrEmpty(e.Id))
                .ToDictionary(e => e.Id);
            return _byId.TryGetValue(id, out var evt) ? evt : null;
        }

#if UNITY_EDITOR
        public void ApplyImport(List<EventData> events)
        {
            Events = events ?? new List<EventData>();
            _byId = null;
        }
#endif
    }
}
