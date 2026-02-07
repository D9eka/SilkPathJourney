using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [CreateAssetMenu(menuName = "SPJ/UI/Resource Icon Catalog")]
    public class ResourceIconCatalog : ScriptableObject
    {
        [field: SerializeField] public Sprite Weight { get; private set; }
        [field: SerializeField] public Sprite Money { get; private set; }
        [field: SerializeField] public Sprite Food { get; private set; }
        [field: SerializeField] public Sprite Danger { get; private set; }
        [field: SerializeField] public Sprite PlayerCartDurability { get; private set; }
        [field: SerializeField] public Sprite OtherCartsDurability { get; private set; }
    }
}
