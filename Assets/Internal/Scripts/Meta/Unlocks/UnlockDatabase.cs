using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Meta.Unlocks
{
    [CreateAssetMenu(menuName = "SPJ/Unlocks/Unlock Database")]
    public sealed class UnlockDatabase : ScriptableObject
    {
        [SerializeField] private UnlockData[] _all = System.Array.Empty<UnlockData>();

        public UnlockData[] All => _all;

#if UNITY_EDITOR
        public void ApplyImport(List<UnlockData> all)
        {
            _all = all?.ToArray() ?? System.Array.Empty<UnlockData>();
        }
#endif
    }
}
