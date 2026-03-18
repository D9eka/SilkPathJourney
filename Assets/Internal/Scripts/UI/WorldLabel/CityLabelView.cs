using Internal.Scripts.UI.WorldLabel.Components;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class CityLabelView : MonoBehaviour
    {
        [field:SerializeField] public NameLabelView _nameLabel;
        [field:SerializeField] public ModifierIconsView Modifiers;
    }
}
