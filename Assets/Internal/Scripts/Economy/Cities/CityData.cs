using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Cities
{
    [CreateAssetMenu(menuName = "SPJ/Economy/City", fileName = "City")]
    public class CityData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string NodeId { get; private set; }
        [field: SerializeField] public CityType Type { get; private set; }
        [field: SerializeField] public CultureId PrimaryCulture { get; private set; }
        [field: SerializeField] public CultureId SecondaryCulture { get; private set; }
        [field: SerializeField] public float MarketScale { get; private set; }
        [field: SerializeField] public bool HasPort { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            string nodeId,
            CityType type,
            CultureId primaryCulture,
            CultureId secondaryCulture,
            float marketScale,
            bool hasPort,
            LocalizedString name)
        {
            Id = id;
            NodeId = nodeId;
            Type = type;
            PrimaryCulture = primaryCulture;
            SecondaryCulture = secondaryCulture;
            MarketScale = marketScale;
            HasPort = hasPort;
            Name = name;
        }
#endif
    }
}
