using System.Collections.Generic;

namespace Internal.Scripts.Save
{
    public interface ISaveService
    {
        List<SaveMetadata> GetAllSaves();
        bool HasAnySave();
        SaveData Load(string slotId);
        void Save(string slotId, SaveData data, SaveMetadata metadata);
        void Delete(string slotId);
    }
}
