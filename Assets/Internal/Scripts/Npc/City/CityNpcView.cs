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
        private NpcLabelFactory _labelHelper;

        public NpcLabelView Label => _labelHelper?.NpcLabel;
        public Sprite QuestIconSprite => _questIconSprite;

        [Inject]
        public void Construct(WorldCanvas worldCanvas)
        {
            _worldCanvas = worldCanvas;
            _labelHelper = new NpcLabelFactory(worldCanvas);
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
            _labelHelper?.Dispose();
        }
    }
}

