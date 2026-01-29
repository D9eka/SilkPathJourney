using System;
using System.IO;
using Internal.Scripts.Economy.Save;
using UnityEngine;

namespace Internal.Scripts.Save
{
    public sealed class JsonSaveService : ISaveService
    {
        private const string SAVE_FILE_NAME = "save.json";
        
        public string SavePath { get; }

        public JsonSaveService()
        {
            SavePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        public bool HasSave() => File.Exists(SavePath);

        public SaveData Load()
        {
            if (!HasSave())
                return null;

            try
            {
                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                    return null;

                if (data.Economy == null && data.Player == null && json.Contains("\"PlayerInventory\""))
                {
                    EconomySaveData legacy = JsonUtility.FromJson<EconomySaveData>(json);
                    if (legacy != null)
                        legacy.IsInitialized = true;
                    return new SaveData
                    {
                        Economy = legacy,
                        Player = new PlayerSaveData()
                    };
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to load save file: {e.Message}");
                return null;
            }
        }

        public void Save(SaveData data)
        {
            if (data == null)
                return;

            try
            {
                string dir = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to save file: {e.Message}");
            }
        }
    }
}
