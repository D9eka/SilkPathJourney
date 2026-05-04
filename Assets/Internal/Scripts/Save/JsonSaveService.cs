using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using UnityEngine;

namespace Internal.Scripts.Save
{
    public sealed class JsonSaveService : ISaveService
    {
        public const string RunSlotId = "run";
        private const string SAVES_FOLDER = "saves";
        private const string LEGACY_SAVE_FILE = "save.json";

        private readonly IJsonStorage _storage;

        public JsonSaveService(IJsonStorage storage)
        {
            _storage = storage;
            MigrateLegacySave();
        }

        public List<SaveMetadata> GetAllSaves()
        {
            var result = new List<SaveMetadata>();

            foreach (string fullPath in _storage.ListFiles(SAVES_FOLDER))
            {
                string slotId = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                var wrapper = _storage.Load<SaveFileWrapper>(RelativeSavePath(slotId));
                if (wrapper?.Metadata != null)
                    result.Add(wrapper.Metadata);
            }

            result.Sort((a, b) => b.LastSavedTimestamp.CompareTo(a.LastSavedTimestamp));
            return result;
        }

        public bool HasAnySave()
        {
            return _storage.ListFiles(SAVES_FOLDER).Count > 0;
        }

        public bool HasActiveRun()
        {
            return _storage.Exists(RelativeSavePath(RunSlotId));
        }

        public void DeleteRun()
        {
            Delete(RunSlotId);
        }

        public SaveData Load(string slotId)
        {
            var wrapper = _storage.Load<SaveFileWrapper>(RelativeSavePath(slotId));
            return wrapper?.Data;
        }

        public void Save(string slotId, SaveData data, SaveMetadata metadata)
        {
            if (data == null || string.IsNullOrEmpty(slotId))
                return;

            var wrapper = new SaveFileWrapper { Metadata = metadata, Data = data };
            _storage.Save(RelativeSavePath(slotId), wrapper);
        }

        public void Delete(string slotId)
        {
            _storage.Delete(RelativeSavePath(slotId));
        }

        private static string RelativeSavePath(string slotId) => $"{SAVES_FOLDER}/{slotId}.json";

        private void MigrateLegacySave()
        {
            if (!_storage.Exists(LEGACY_SAVE_FILE))
                return;

            try
            {
                var data = _storage.Load<SaveData>(LEGACY_SAVE_FILE);
                if (data == null)
                {
                    var legacy = _storage.Load<EconomySaveData>(LEGACY_SAVE_FILE);
                    if (legacy != null)
                    {
                        legacy.IsInitialized = true;
                        data = new SaveData { Economy = legacy, Player = new PlayerSaveData() };
                    }
                    else
                    {
                        _storage.Delete(LEGACY_SAVE_FILE);
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
                _storage.Delete(LEGACY_SAVE_FILE);
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
