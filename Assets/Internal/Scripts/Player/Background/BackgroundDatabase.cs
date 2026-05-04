using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Internal.Scripts.Player.Background
{
    [CreateAssetMenu(menuName = "SPJ/Player/Background Database", fileName = "BackgroundDatabase")]
    public class BackgroundDatabase : ScriptableObject
    {
        [field: SerializeField] public List<BackgroundData> Backgrounds { get; private set; } = new();

        private Dictionary<string, BackgroundData> _byId;

        public BackgroundData GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _byId ??= Backgrounds.ToDictionary(b => b.Id);
            return _byId.TryGetValue(id, out var bg) ? bg : null;
        }

        public BackgroundData GetFirstUnlocked()
        {
            return Backgrounds.Find(b => b.IsUnlockedByDefault);
        }

#if UNITY_EDITOR
        public void ApplyImport(List<BackgroundData> backgrounds)
        {
            Backgrounds = backgrounds ?? new List<BackgroundData>();
            _byId = null;
        }
#endif
    }
}
