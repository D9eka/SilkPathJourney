using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.UI.Screen.Config
{
    [CreateAssetMenu(menuName = "SPJ/UI/Screen Config", fileName = "ScreenConfig")]
    public sealed class ScreenConfig : ScriptableObject
    {
        [field: SerializeField] public ScreenId Id { get; private set; } = ScreenId.None;
        [field: SerializeField] public int SortOrder { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public bool IsExclusive { get; private set; }
        [field: SerializeField] public List<ScreenId> AllowedToOpenWhileExclusive { get; private set; } = new();
        [field: SerializeField] public bool BlocksWorldInput { get; private set; } = true;
        [field: SerializeField] public bool BlocksUIUnderneath { get; private set; } = true;
        [field: SerializeField] public bool CloseOnBack { get; private set; } = true;
    }
}
