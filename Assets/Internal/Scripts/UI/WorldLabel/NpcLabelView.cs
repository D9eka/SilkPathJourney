using Internal.Scripts.UI.WorldLabel.Components;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class NpcLabelView : MonoBehaviour
    {
        [field:SerializeField] public NameLabelView _nameLabel;
        
        public void SetIcon(Sprite sprite) => _nameLabel.SetIcon(sprite);
        public void HideIcon() => _nameLabel.HideIcon();
        public void Show() => _nameLabel.Show();
        public void Hide() => _nameLabel.Hide();
    }
}
