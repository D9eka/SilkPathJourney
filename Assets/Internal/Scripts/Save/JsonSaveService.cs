using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Internal.Scripts.Economy.Save;
using UnityEngine;

namespace Internal.Scripts.Save
{
    public sealed class JsonSaveService : ISaveService
    {
        private const string SAVES_FOLDER = "saves";
        private const string LEGACY_SAVE_FILE = "save.json";

        private readonly string _savesDir;

        public JsonSaveService()
        {
            _savesDir = Path.Combine(Application.persistentDataPath, SAVES_FOLDER);
            Directory.CreateDirectory(_savesDir);
            MigrateLegacySave();
        }

        public List<SaveMetadata> GetAllSaves()
        {
            var result = new List<SaveMetadata>();

            if (!Directory.Exists(_savesDir))
                return result;

            foreach (string file in Directory.GetFiles(_savesDir, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var wrapper = JsonUtility.FromJson<SaveFileWrapper>(json);
                    if (wrapper?.Metadata != null)
                        result.Add(wrapper.Metadata);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SPJ] Failed to read save metadata from {file}: {e.Message}");
                }
            }

            result.Sort((a, b) => b.LastSavedTimestamp.CompareTo(a.LastSavedTimestamp));
            return result;
        }

        public bool HasAnySave()
        {
            return Directory.Exists(_savesDir) && Directory.GetFiles(_savesDir, "*.json").Length > 0;
        }

        public SaveData Load(string slotId)
        {
            string path = GetSlotPath(slotId);

            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                var wrapper = JsonUtility.FromJson<SaveFileWrapper>(json);
                return wrapper?.Data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to load save slot {slotId}: {e.Message}");
                return null;
            }
        }

        public void Save(string slotId, SaveData data, SaveMetadata metadata)
        {
            if (data == null || string.IsNullOrEmpty(slotId))
                return;

            try
            {
                Directory.CreateDirectory(_savesDir);

                var wrapper = new SaveFileWrapper
                {
                    Metadata = metadata,
                    Data = data
                };

                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(GetSlotPath(slotId), json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to save slot {slotId}: {e.Message}");
            }
        }

        public void Delete(string slotId)
        {
            string path = GetSlotPath(slotId);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetSlotPath(string slotId)
        {
            return Path.Combine(_savesDir, slotId + ".json");
        }

        private void MigrateLegacySave()
        {
            string legacyPath = Path.Combine(Application.persistentDataPath, LEGACY_SAVE_FILE);
            if (!File.Exists(legacyPath))
                return;

            try
            {
                string json = File.ReadAllText(legacyPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    File.Delete(legacyPath);
                    return;
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    if (json.Contains("\"PlayerInventory\""))
                    {
                        EconomySaveData legacy = JsonUtility.FromJson<EconomySaveData>(json);
                        if (legacy != null)
                            legacy.IsInitialized = true;
                        data = new SaveData
                        {
                            Economy = legacy,
                            Player = new PlayerSaveData()
                        };
                    }
                    else
                    {
                        File.Delete(legacyPath);
                        return;
                    }
                }

                string slotId = Guid.NewGuid().ToString("N");
                var metadata = new SaveMetadata
                {
                    SlotId = slotId,
                    DisplayName = $"День {data.Player?.CurrentDay ?? 1}",
                    LastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    CurrentDay = data.Player?.CurrentDay ?? 1
                };

                Save(slotId, data, metadata);
                File.Delete(legacyPath);
                Debug.Log($"[SPJ] Migrated legacy save to slot {slotId}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to migrate legacy save: {e.Message}");
            }
        }

        [Serializable]
        private class SaveFileWrapper
        {
            public SaveMetadata Metadata;
            public SaveData Data;
        }
    }
}
