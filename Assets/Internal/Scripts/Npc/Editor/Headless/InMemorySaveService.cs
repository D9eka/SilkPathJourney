using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Save;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class InMemorySaveService : ISaveService
    {
        private readonly Dictionary<string, (SaveData data, SaveMetadata metadata)> _store = new();

        public List<SaveMetadata> GetAllSaves() => _store.Values.Select(e => e.metadata).ToList();

        public bool HasAnySave() => _store.Count > 0;

        public bool HasActiveRun() => _store.ContainsKey(JsonSaveService.RunSlotId);

        public void DeleteRun() => Delete(JsonSaveService.RunSlotId);

        public SaveData Load(string slotId)
        {
            if (_store.TryGetValue(slotId, out var entry))
                return entry.data;
            return new SaveData();
        }

        public void Save(string slotId, SaveData data, SaveMetadata metadata)
        {
            _store[slotId] = (data, metadata);
        }

        public void Delete(string slotId) => _store.Remove(slotId);
    }
}
