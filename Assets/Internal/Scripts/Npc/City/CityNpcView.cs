using Internal.Scripts.UI.WorldLabel;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Npc.City
{
    public class CityNpcView : MonoBehaviour
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _questIconSprite;

        private WorldCanvas _worldCanvas;
        private WorldLabelViewHelper _labelHelper;

        public WorldLabelView Label => _labelHelper?.Label;
        public Sprite QuestIconSprite => _questIconSprite;

        [Inject]
        public void Construct(WorldCanvas worldCanvas)
        {
            _worldCanvas = worldCanvas;
            _labelHelper = new WorldLabelViewHelper(worldCanvas);
        }

        private void Start()
        {
            var label = _labelHelper.CreateLabel(
                transform.position,
                $"NpcLabel_{_displayName}",
                _displayName);
            label?.HideIcon();
        }

        private void OnDestroy()
        {
            _labelHelper?.Dispose();
        }
    }
}

