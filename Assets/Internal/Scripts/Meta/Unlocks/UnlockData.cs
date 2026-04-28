using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Meta.Unlocks
{
    [CreateAssetMenu(menuName = "SPJ/Unlocks/Unlock")]
    public sealed class UnlockData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public UnlockCategory Category { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

#if UNITY_EDITOR
        public void ApplyImport(string id, UnlockCategory category, LocalizedString name,
            LocalizedString description, int cost, Sprite icon)
        {
            Id = id;
            Category = category;
            Name = name;
            Description = description;
            Cost = cost;
            Icon = icon;
        }
#endif
    }
}
