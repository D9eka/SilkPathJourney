using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Internal.Scripts.Save
{
    public sealed class JsonFileStorage : IJsonStorage
    {
        private readonly string _root;

        public JsonFileStorage()
        {
            _root = Application.persistentDataPath;
        }

        public bool Exists(string relativePath)
        {
            return File.Exists(FullPath(relativePath));
        }

        public T Load<T>(string relativePath) where T : class
        {
            string path = FullPath(relativePath);
            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return null;
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to load {relativePath}: {e.Message}");
                return null;
            }
        }

        public void Save<T>(string relativePath, T data, bool prettyPrint = true)
        {
            string path = FullPath(relativePath);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonUtility.ToJson(data, prettyPrint));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to save {relativePath}: {e.Message}");
            }
        }

        public void Delete(string relativePath)
        {
            string path = FullPath(relativePath);
            if (File.Exists(path))
                File.Delete(path);
        }

        public IReadOnlyList<string> ListFiles(string relativeDir, string searchPattern = "*.json")
        {
            string dir = FullPath(relativeDir);
            if (!Directory.Exists(dir))
                return Array.Empty<string>();

            return Directory.GetFiles(dir, searchPattern);
        }

        private string FullPath(string relativePath) => Path.Combine(_root, relativePath);
    }
}
