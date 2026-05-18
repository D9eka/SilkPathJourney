using Internal.Scripts.UI.Tooltip;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel.Components
{
    public class ModifierIconsView : MonoBehaviour
    {
        [SerializeField] private RectTransform _iconsContainer;
        [SerializeField] private WorldLabelIcon _iconTemplate;
        [SerializeField] private GameObject _background;

        private TooltipService _tooltipService;

        private void Awake()
        {
            SetVisible(false);
        }

        public void Initialize(TooltipService tooltipService)
        {
            _tooltipService = tooltipService;
        }

        public void ClearIcons()
        {
            if (_iconsContainer == null) return;
            for (int i = _iconsContainer.childCount - 1; i >= 0; i--)
            {
                var child = _iconsContainer.GetChild(i);
                if (child.GetComponent<WorldLabelIcon>() != null)
                    Destroy(child.gameObject);
            }
        }

        public void AddIcon(Sprite icon, string name, string description, float alpha = 1f)
        {
            if (_iconTemplate == null || _iconsContainer == null) return;

            SetVisible(true);
            var labelIcon = Instantiate(_iconTemplate, _iconsContainer);
            labelIcon.Initialize(_tooltipService, name, description, icon, alpha);
        }

        public void SetVisible(bool visible)
        {
            if (_iconsContainer != null)
                _iconsContainer.gameObject.SetActive(visible);
            if (_background != null)
                _background.SetActive(visible);
        }
    }
}
