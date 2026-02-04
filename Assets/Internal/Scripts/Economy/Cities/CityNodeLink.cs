using UnityEngine;

namespace Internal.Scripts.Economy.Cities
{
    public sealed class CityNodeLink : MonoBehaviour
    {
        [field: SerializeField] public string CityId { get; private set; }
        [field: SerializeField] public CityData City { get; private set; }

#if UNITY_EDITOR
        public void ApplyLink(CityData city)
        {
            City = city;
            CityId = city != null ? city.Id : string.Empty;
        }
#endif
    }
}
