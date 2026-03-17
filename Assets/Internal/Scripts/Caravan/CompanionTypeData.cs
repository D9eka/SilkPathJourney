using Internal.Scripts.Caravan.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Caravan
{
    [CreateAssetMenu(menuName = "SPJ/Caravan/Companion Type", fileName = "CompanionType")]
    public class CompanionTypeData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public CompanionType Type { get; private set; }
        [field: SerializeField] public int HireCostBase { get; private set; }
        [field: SerializeField] public int DailyCostBase { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(string id, CompanionType type, int hireCostBase, int dailyCostBase, LocalizedString name)
        {
            Id = id;
            Type = type;
            HireCostBase = hireCostBase;
            DailyCostBase = dailyCostBase;
            Name = name;
        }
#endif
    }
}
