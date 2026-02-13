using Internal.Scripts.UI.WorldLabel;
using UnityEngine;

namespace Internal.Scripts.UI.Arrow.Placement
{
    public class ArrowFactory
    {
        private readonly WorldCanvas _worldCanvas;
        private readonly Sprite _arrowSprite;

        public ArrowFactory(WorldCanvas worldCanvas, Sprite arrowSprite)
        {
            _worldCanvas = worldCanvas;
            _arrowSprite = arrowSprite;
        }

        public ArrowView CreateArrow(Vector3 worldPosition, string name)
        {
            WorldLabelView labelView = _worldCanvas.CreateLabel(worldPosition, name);
            if (labelView == null) return null;

            ArrowView arrowView = labelView.gameObject.AddComponent<ArrowView>();
            arrowView.InjectDependencies(labelView.IconImage, labelView.NameText, _arrowSprite);
            labelView.IconImage.gameObject.SetActive(true);
            labelView.NameText.gameObject.SetActive(false);

            return arrowView;
        }
    }
}
