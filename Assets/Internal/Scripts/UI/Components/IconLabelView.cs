using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    public class IconLabelView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _label;

        public void Initialize(Sprite icon, string label)
        {
            if (icon != null)
                _icon.sprite = icon;
            SetLabel(label);
        }

        public void SetLabel(string label)
        {
            _label.text = label ?? string.Empty;
        }
    }
}
