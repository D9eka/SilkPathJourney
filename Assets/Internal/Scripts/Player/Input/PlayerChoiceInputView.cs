using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Player.UI.Arrow;
using Internal.Scripts.Player.UI.Arrow.Controller;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Player.Input
{
    public class PlayerChoiceInputView : IPlayerChoiceInput
    {
        protected readonly IArrowsController _arrows;

        private UniTaskCompletionSource<RoadPathSegment> _tcs;
        private List<ArrowView> _arrowsToSubscribe;

        public PlayerChoiceInputView(IArrowsController arrows)
        {
            _arrows = arrows;
        }

        public async UniTask<RoadPathSegment> WaitForChoiceAsync(CancellationToken ct)
        {
            _tcs = new UniTaskCompletionSource<RoadPathSegment>();
            _arrowsToSubscribe = _arrows.GetAllArrows();
            while (_arrowsToSubscribe.Count == 0)
            {
                await UniTask.WaitForFixedUpdate();
                _arrowsToSubscribe = _arrows.GetAllArrows();
            }
            foreach (ArrowView arrow in _arrowsToSubscribe)
            {
                arrow.OnClick += OnArrowClick;
            }
            
            using (ct.Register(() => _arrowsToSubscribe.Clear()))
            {
                return await _tcs.Task;
            }
        }

        private void OnArrowClick(IInteractableObject obj)
        {
            if (obj is not ArrowView arrowView)
            {
                return;
            }
            foreach (ArrowView arrow in _arrowsToSubscribe)
            {
                arrow.OnClick -= OnArrowClick;
            }
            _tcs.TrySetResult(arrowView.Segment);
        }
    }
    
    public interface IPlayerChoiceInput
    {
        UniTask<RoadPathSegment> WaitForChoiceAsync(CancellationToken ct);
    }
}