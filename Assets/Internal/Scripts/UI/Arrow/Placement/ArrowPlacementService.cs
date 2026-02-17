using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Internal.Scripts.UI.Arrow.Placement
{
    public sealed class ArrowPlacementService : IArrowPlacementService
    {
        private const float SPAWN_ANIMATION_DURATION = 0.3f;
        private const float HIDE_ANIMATION_DURATION = 0.2f;

        private readonly ArrowFactory _arrowFactory;
        private readonly UnityEngine.Camera _camera;
        private readonly List<ArrowView> _activeArrows = new();

        public ArrowPlacementService(ArrowFactory arrowFactory, UnityEngine.Camera camera)
        {
            _arrowFactory = arrowFactory;
            _camera = camera;
        }

        public void PlaceArrows(List<ArrowData> arrowDataList)
        {
            ClearArrows();

            foreach (ArrowData data in arrowDataList)
            {
                PlaceArrow(data);
            }
        }

        public void HideArrows()
        {
            foreach (ArrowView arrow in _activeArrows)
            {
                if (arrow != null)
                {
                    GameObject root = arrow.RootObject != null ? arrow.RootObject : arrow.gameObject;
                    arrow.transform.DOScale(Vector3.zero, HIDE_ANIMATION_DURATION)
                        .OnComplete(() => Object.Destroy(root));
                }
            }
            _activeArrows.Clear();
        }

        public List<ArrowView> GetAllArrows() => _activeArrows;

        public void ClearArrows()
        {
            foreach (ArrowView arrow in _activeArrows)
            {
                if (arrow != null)
                {
                    GameObject root = arrow.RootObject != null ? arrow.RootObject : arrow.gameObject;
                    Object.Destroy(root);
                }
            }
            _activeArrows.Clear();
        }

        private void PlaceArrow(ArrowData data)
        {
            ArrowView arrow = _arrowFactory.CreateArrow(data.WorldPos, $"Arrow_{data.Segment.SegmentId}");
            arrow.Initialize(data.Segment, data.Type);
            arrow.SetDirection(data.WorldDir, _camera);

            _activeArrows.Add(arrow);
        }
    }
}