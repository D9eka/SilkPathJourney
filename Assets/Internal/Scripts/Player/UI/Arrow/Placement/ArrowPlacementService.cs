using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Internal.Scripts.Player.UI.Arrow.Placement
{
    public sealed class ArrowPlacementService : IArrowPlacementService
    {
        private const float SPAWN_ANIMATION_DURATION = 0.3f;
        private const float HIDE_ANIMATION_DURATION = 0.2f;

        private readonly Transform _spawnParent;
        private readonly ArrowView _arrowPrefab;
        private readonly List<ArrowView> _activeArrows = new();

        public ArrowPlacementService(Transform spawnParent, ArrowView arrowPrefab)
        {
            _spawnParent = spawnParent;
            _arrowPrefab = arrowPrefab;
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
                    arrow.transform.DOScale(Vector3.zero, HIDE_ANIMATION_DURATION)
                        .OnComplete(() => Object.Destroy(arrow.gameObject));
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
                    Object.Destroy(arrow.gameObject);
            }
            _activeArrows.Clear();
        }

        private void PlaceArrow(ArrowData data)
        {
            ArrowView arrow = Object.Instantiate(_arrowPrefab, _spawnParent);
            arrow.Initialize(data.Segment, data.Type);
            arrow.transform.position = data.WorldPos;
            arrow.SetDirection(data.WorldDir);

            _activeArrows.Add(arrow);
        }
    }
}