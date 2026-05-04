using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Meta.Achievements
{
    [CreateAssetMenu(menuName = "SPJ/Achievements/Achievement Database")]
    public sealed class AchievementDatabase : ScriptableObject
    {
        [SerializeField] private AchievementData[] _all = System.Array.Empty<AchievementData>();

        public AchievementData[] All => _all;

#if UNITY_EDITOR
        public void ApplyImport(List<AchievementData> all)
        {
            _all = all?.ToArray() ?? System.Array.Empty<AchievementData>();
        }
#endif
    }
}
