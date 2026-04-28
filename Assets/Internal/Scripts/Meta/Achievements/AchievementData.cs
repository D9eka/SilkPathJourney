using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Meta.Achievements
{
    [CreateAssetMenu(menuName = "SPJ/Achievements/Achievement")]
    public sealed class AchievementData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public AchievementTrigger Trigger { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
        [field: SerializeField] public int LegacyReward { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

#if UNITY_EDITOR
        public void ApplyImport(string id, LocalizedString name, LocalizedString description,
            AchievementTrigger trigger, int value, int legacyReward, Sprite icon)
        {
            Id = id;
            Name = name;
            Description = description;
            Trigger = trigger;
            Value = value;
            LegacyReward = legacyReward;
            Icon = icon;
        }
#endif
    }
}
