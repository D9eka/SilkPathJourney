using Internal.Scripts.UI.WorldLabel;
using UnityEngine;

namespace Internal.Scripts.UI.Arrow.Placement
{
    public class ArrowFactory
    {
        private readonly WorldCanvas _worldCanvas;
        private readonly ArrowView _arrowPrefab;

        public ArrowFactory(WorldCanvas worldCanvas, ArrowView arrowPrefab)
        {
            _worldCanvas = worldCanvas;
            _arrowPrefab = arrowPrefab;
        }

        public ArrowView CreateArrow(Vector3 worldPosition, string name)
        {
            WorldLabelView labelView = _worldCanvas.CreateLabel(worldPosition, name);
            if (labelView == null) return null;

            labelView.NameText.gameObject.SetActive(false);

            ArrowView arrow = Object.Instantiate(_arrowPrefab, labelView.transform);
            arrow.transform.localPosition = Vector3.zero;
            arrow.RootObject = labelView.gameObject;

            return arrow;
        }
    }
}
