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
            RectTransform root = _worldCanvas.CreatePositionedRoot(worldPosition, Vector3.zero, name);
            if (root == null) return null;

            _worldCanvas.AddBillboard(root.gameObject);

            ArrowView arrow = Object.Instantiate(_arrowPrefab, root);
            arrow.transform.localPosition = Vector3.zero;
            arrow.RootObject = root.gameObject;

            return arrow;
        }
    }
}
