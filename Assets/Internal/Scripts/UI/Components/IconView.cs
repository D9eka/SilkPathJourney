using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Components
{
    public class IconView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private ResourceType _resourceType;

        [Inject(Optional = true)] private ResourceIconCatalog _catalog;

        private void Awake()
        {
            ApplyFromCatalog();
        }

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void SetResourceType(ResourceType type)
        {
            _resourceType = type;
            ApplyFromCatalog();
        }

        private void ApplyFromCatalog()
        {
            if (_catalog == null) return;
            ResourceEntry entry = _catalog.Get(_resourceType);
            if (entry != null) _image.sprite = entry.Icon;
        }
    }
}
