using System.Collections.Generic;

namespace Internal.Scripts.Save
{
    public interface IJsonStorage
    {
        bool Exists(string relativePath);
        T Load<T>(string relativePath) where T : class;
        void Save<T>(string relativePath, T data, bool prettyPrint = true);
        void Delete(string relativePath);
        IReadOnlyList<string> ListFiles(string relativeDir, string searchPattern = "*.json");
    }
}
